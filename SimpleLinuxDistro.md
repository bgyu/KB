# Create a simple Linux Distro

### Three major components for a simple Linux Distro
* Bootloader: Boots Linux Kernel
* Linux Kernel: The Linux Kernel Itself
* Init: First user process

### Prerequistes
* You have a ready to use Linux system, VM, container or a real system whatever
* Install docker or podman
We don't want to affect the current system, so we make everything inside a container.
Assume you are using `podman`, if you are using `docker`, you can just create an alias: `alias podman=docker`.

### Step by Step Guide
#### Clone Linux Kernel And Compile the Kernel
```
podman run -it ubuntu  # Start the container
```
Now we are inside the container `Ubuntu` system with root account:
```
# Install required tools
apt install -y bzip2 git vim make gcc libncurses-dev flex bison bc cpio libelf-dev libssl-dev dosfstools busybox syslinux

# Clone linux Kernel
mkdir /source
cd /source
git clone --depth 1 https://github.com/torvalds/linux.git
cd linux
make defconfig
make -j $(nproc)

# Create a new folder distro
mkdir /distro
# Copy the new built kernel
cp arch/x86_64/boot/bzImage /distro/
```

### Create File system (initramfs)
```
mkdir /distro/initramfs
cd /distro/initramfs
mkdir -p bin sbin usr/bin usr/sbin
cp /usr/bin/busybox ./bin/

cd bin
for cmd in $(./busybox --list); do
    ln -s busybox $cmd
done

cd .. # /distro/initramfs
mkdir - dev proc sys tmp etc lib lib64 mnt root
chmod 1777 tmp

# Create rootfs (initramfs)
find . | cpio -o --format newc | gzip > ../initramfs.img
```
### Create bootable disk
```
cd /distro
dd if=/dev/zero of=mylinux.img bs=1M count=50  # Create virtual disk
mkfs -t fat mylinux.img # format the disk
syslinux mylinux.img # install bootloader to the disk
```
### Copy Kernel to the bootable disk
Becasue we don't have permission (with rootless podman) to mount inside the container,
we need to copy everything out from the container and mount mylinux.img and then copy the Linux kernel.
Assuming the container Id is 7aedd9f76764. You can get it with `podman ps`.
In a new terminal:
```
podman ps -- get container id
# Copy the whole distro folder to podman machine
podman cp 7aedd9f76764:/distro .
cd ./distro
mkdir mnt
sudo mount mylinux.img ./mnt
sudo cp bzImage initramfs.img ./mnt  # Copy Linux Kernel to the bootable disk
sudo umount mnt
```
