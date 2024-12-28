# Download Linux Kernel Source Code
```
git clone https://github.com/torvalds/linux.git

# Can use shallow clone single branch with latest commit instead of all branches with full history to reduce time
# git clone --depth 1 --branch master https://git.kernel.org/pub/scm/linux/kernel/git/torvalds/linux.git
```

# Configure Linux Kernel To Enable Debugging
```
make menuconfig
```
Use `/` to search `debug_info`, then type `1` to choose the first result. It will bring you to the configure menu "Compile-time checks and compiler options".
Press *Enter* to go to "Debug information"; press *Enter* again, select "Rely on the toolchain's implicit default DWARF version" by press "Enter" again.
Then select "Provide GDB scripts for kernel debugging". "Save" and "Exit" the configuration.

# Build The Kernel
```
# Build the kernel with all CPU cores
make -j $(nproc)
```
# Configure gdb
```
make scripts_gdb
```
Add the following configuration to *~/.gdbinit*:
```
add-auto-load-safe-path <path-to-linux-kernel>
```
Replace `<path-to-linux-kernel>` with the actual linux kernel path.

# Debug Linux Kernel
## Start new built kernel with qemu
```
# start qemu in graphic mode
qemu-system-x86_64 -kernel arch/x86_64/boot/bzImage -s -S -append nokaslr

# start qemu in console mode
qemu-system-x86_64 -kernel arch/x86_64/boot/bzImage -s -S -append "nokaslr console=ttyS0,115200 earlyprintk=serial,ttyS0,115200" -nographic
```
This will start the new kernel with qemu and waiting for debugger to connect.
Note: If you start qemu in nographic mode, you need to specify "console" parameters, otherwise you won't see any output.

### Import parameters for debugging
* `-s`: Starts a GDB server on port 1234
* `-S`: Stops the CPU at startup unit GDB continues execution
* `nokaslr`: a kernel boot parameter used to disable Kernel Address Space Layout Randomization (KASLR). `KASLR` is a security feature. We need to disable it so that the breakpoint works.

## Start gdb to debug the kernel
```
gdb vmlinux  # start the kernel with gdb
(gdb) target remote :1234  # Connect to gdb server
(gdb) hbreak kernel_init # Set a hard breakpoint in `kernel_init`
(gdb) c # running
(gdb) n # next step
(gdb) s # step into
```
You can change the code and rebuild the kernel and check the changes. 
For example, inside `start_kernel` function, after `console_init();`, you can add the following line:
```
printk(KERN_DEBUG "Hello, Welcome to Linux");
```
You should be able to see the output when the line gets executed.

# References
* https://docs.kernel.org/dev-tools/gdb-kernel-debugging.html
* https://github.com/torvalds/linux

# Futher: Fix Kernel Panic by Providing rootfs - Use initramfs
Because we don't provide rootfs, kernel will panic at the end. To have a "complete" system, we need to provide a rootfs. Here are the detailed steps:

Creating a minimal root filesystem using `busybox` and passing it to the Linux kernel involves the following steps:

---

### Step 1: **Install BusyBox**
Make sure `busybox` is installed on your system:
```bash
sudo dnf install busybox
```

---

### Step 2: **Create a Minimal Root Filesystem**
1. **Set Up a Working Directory**:
   Create a directory to hold the root filesystem:
   ```bash
   mkdir rootfs
   cd rootfs
   ```

2. **Install BusyBox**:
   Use `busybox` to populate the root filesystem:
   ```bash
   mkdir -p bin sbin usr/bin usr/sbin
   cp /usr/bin/busybox bin/
   ```

3. **Set Up Symlinks**:
   Inside `rootfs/bin`, create symlinks for the commands provided by `busybox`:
   ```bash
   cd bin
   for cmd in $(./busybox --list); do
       ln -s busybox $cmd
   done
   cd ../../
   ```

4. **Create Essential Directories**:
   Add directories required by the Linux kernel:
   ```bash
   mkdir -p dev proc sys tmp etc lib lib64 mnt root
   chmod 1777 tmp  # Make tmp world-writable
   ```

5. **Add `init` Script**:
   Create an `init` script in `rootfs/init` (this acts as the root initialization script):
   ```bash
   cat > init << 'EOF'
   #!/bin/sh
   mount -t proc none /proc
   mount -t sysfs none /sys
   echo "Welcome to BusyBox root filesystem!"
   exec /bin/sh
   EOF
   chmod +x init
   ```

---

### Step 3: **Create the Root Filesystem Image**
1. **Pack the Root Filesystem into a CPIO Archive**:
   Create a `cpio` archive and compress it:
   ```bash
   find . | cpio -o --format=newc | gzip > ../rootfs.img
   ```

2. **Verify the Archive**:
   Ensure the archive was created successfully:
   ```bash
   zcat ../rootfs.img | cpio -t
   ```

---

### Step 4: **Pass the Filesystem to the Kernel**
Use the `rootfs.img` as the `initramfs` when booting the kernel in QEMU:
   ```bash
  qemu-system-x86_64 -kernel arch/x86_64/boot/bzImage -s -S -append "nokaslr console=ttyS0,115200 earlyprintk=serial,ttyS0,115200" -nographic -initrd rootfs.img
   ```

---

### Debugging
1. If the kernel cannot find the `init` script:
   - Ensure it is executable (`chmod +x init`).
   - Verify the path (`/init`) in the `initramfs`.
   - Check for errors in the kernel output.

2. If busybox commands are not available:
   - Verify symlinks in `bin/`.

---

### Final Notes
- This setup creates a very minimal environment. You can add more binaries, libraries, or configurations as needed for your specific use case.
- Test thoroughly with QEMU to ensure the kernel boots successfully with the new root filesystem.

# Futher: Fix Kernel Panic by Providing rootfs - Use real root filesystem
To create a **minimal real filesystem** instead of using an `initramfs`. This involves creating a disk image, formatting it with a filesystem (e.g., `ext4`), and populating it with the required files and directories. This disk image can then be passed to the Linux kernel via QEMU as a block device (`-drive` option) to be mounted as the root filesystem.

Here's how to do it step-by-step:

---

### **Step 1: Create a Disk Image**
1. **Create an Empty Disk Image**:
   ```bash
   dd if=/dev/zero of=filesystem.img bs=1M count=64
   ```
   - `filesystem.img`: Name of the disk image.
   - `bs=1M count=64`: Creates a 64MB disk image (adjust size as needed).

2. **Format the Disk Image with a Filesystem**:
   ```bash
   mkfs.ext4 filesystem.img
   ```

---

### **Step 2: Mount the Disk Image**
1. **Create a Mount Point**:
   ```bash
   mkdir mnt
   ```

2. **Mount the Disk Image**:
   Use `loop` to mount the image:
   ```bash
   sudo mount -o loop filesystem.img mnt
   ```

---

### **Step 3: Populate the Filesystem**
1. **Set Up Basic Directories**:
   Create essential directories inside the mounted image:
   ```bash
   sudo mkdir -p mnt/bin mnt/sbin mnt/usr/bin mnt/usr/sbin mnt/dev mnt/proc mnt/sys mnt/tmp mnt/etc mnt/lib mnt/lib64 mnt/root
   sudo chmod 1777 mnt/tmp  # Make /tmp writable
   ```

2. **Copy BusyBox**:
   Install `busybox` into the filesystem:
   ```bash
   sudo cp /usr/bin/busybox mnt/bin/
   ```

3. **Set Up Symlinks for BusyBox**:
   Create symlinks for `busybox` commands:
   ```bash
   sudo chroot mnt /bin/busybox --install -s
   ```

4. **Create the `init` Script**:
   Create an `init` script for the root filesystem:
   ```bash
   sudo bash -c 'cat > mnt/init <<EOF
   #!/bin/sh
   mount -t proc none /proc
   mount -t sysfs none /sys
   echo "Welcome to the minimal real filesystem!"
   exec /bin/sh
   EOF'
   sudo chmod +x mnt/init
   ```

5. **Create Device Nodes**:
   Use `mknod` to create basic device nodes:
   ```bash
   sudo mknod mnt/dev/null c 1 3
   sudo mknod mnt/dev/console c 5 1
   ```

---

### **Step 4: Unmount the Disk Image**
Once the filesystem is set up:
```bash
sudo umount mnt
```

---

### **Step 5: Boot the Kernel with the Real Filesystem**
Use QEMU to boot the kernel and specify the disk image as the root filesystem:
```bash
qemu-system-x86_64 -kernel arch/x86_64/boot/bzImage -s -S -append "nokaslr root=/dev/sda rw console=ttyS0" -drive file=filesystem.img,format=raw,if=ide -nographic
```

- `root=/dev/sda`: Specifies the root filesystem is on `/dev/sda`.
- `-drive`: Passes the disk image to QEMU as a virtual block device.

---

This approach creates a minimal real filesystem that the kernel can mount as `/`. It avoids using `initramfs` entirely, relying instead on a block device (`filesystem.img`) to serve as the root filesystem.
Also when you type `exit` in the console, the kernel won't panic like initramfs, you can press Enter to go to shell.
