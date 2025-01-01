# Build a minimu Linux System 

Building a minimum Linux system that can be booted using QEMU involves creating a small root filesystem, configuring the kernel boot parameters, and running the system in QEMU. Below are the detailed steps:

### 1. **Setup Environment**
Make sure you have the following installed on your host system:
- QEMU
- GNU Core Utilities (for building the root filesystem)
- BusyBox
- A Linux kernel bzImage
- Cross-compiler (if building for a different architecture)

### 2. **Create a Root Filesystem**
#### 2.1 Prepare Directory Structure
```bash
mkdir -p rootfs/{bin,sbin,etc,proc,sys,dev,tmp,var,mnt,lib,usr/{bin,sbin}}
```

#### 2.2 Compile and Install BusyBox
BusyBox provides minimal versions of standard Unix utilities.
```bash
wget https://busybox.net/downloads/busybox-<version>.tar.bz2
tar xjf busybox-<version>.tar.bz2
cd busybox-<version>
make defconfig
make -j$(nproc)
make install CONFIG_PREFIX=../rootfs
cd ..
```

#### 2.3 Configure `/init`
Create an `init` script that acts as the init system:
```bash
cat > rootfs/init << EOF
#!/bin/sh
mount -t proc none /proc
mount -t sysfs none /sys
echo "Welcome to the minimal Linux system!"
/bin/sh
EOF
chmod +x rootfs/init
```

#### 2.4 Create Device Nodes
```bash
mknod rootfs/dev/console c 5 1
mknod rootfs/dev/null c 1 3
```

---

### 3. **Prepare Root Filesystem Image**
#### 3.1 Create a CPIO Archive
```bash
cd rootfs
find . | cpio -o --format=newc > ../rootfs.cpio
cd ..
```

#### 3.2 Compress the Archive (Optional)
```bash
gzip rootfs.cpio
```

---

### 4. **Run with QEMU**
#### 4.1 Command to Boot
Replace `<path_to_bzImage>` and `<path_to_rootfs.cpio.gz>` with appropriate paths:
```bash
qemu-system-x86_64 \
    -kernel <path_to_bzImage> \
    -initrd <path_to_rootfs.cpio.gz> \
    -append "console=ttyS0" \
    -nographic
```

#### 4.2 Explanation
- `-kernel`: Specifies the kernel image to boot.
- `-initrd`: Specifies the initial root filesystem.
- `-append "console=ttyS0"`: Directs kernel logs to the serial console.
- `-nographic`: Runs QEMU in headless mode.

---

### 5. **Testing and Debugging**
- If the system boots successfully, you should see a shell prompt.
- If there are errors, check the following:
  - Device nodes in `/dev`.
  - Correctly mounted `/proc` and `/sys`.
  - Kernel logs for missing modules or configuration.
