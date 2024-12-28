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
Replace "<path-to-linux-kernel>" to the actual linux kernel path.

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

# Futher: Fix Kernel Panic by Providing rootfs
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

