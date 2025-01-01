Below is a list of common Linux kernel parameters and their meanings. Kernel parameters are passed by the bootloader (e.g., GRUB, LILO) to control kernel behavior during boot.

---

### **Categories of Kernel Parameters**

#### 1. **General Parameters**
| Parameter        | Description                                                                 |
|------------------|-----------------------------------------------------------------------------|
| `quiet`          | Suppresses most kernel boot messages.                                      |
| `verbose`        | Outputs detailed kernel messages (opposite of `quiet`).                   |
| `loglevel=`      | Sets the kernel logging level (0-7, where 0 is the least verbose).         |
| `init=`          | Specifies the first user-space program to execute (default is `/sbin/init`).|
| `root=`          | Specifies the device or filesystem to use as the root filesystem.         |
| `ro`             | Mounts the root filesystem as read-only (default).                        |
| `rw`             | Mounts the root filesystem as read-write.                                 |
| `noinitrd`       | Disables initial RAM disk (initrd) support.                               |

---

#### 2. **Memory and Processor Parameters**
| Parameter           | Description                                                                 |
|---------------------|-----------------------------------------------------------------------------|
| `mem=`              | Limits the amount of memory the kernel uses (e.g., `mem=512M`).           |
| `nosmp`             | Disables symmetric multiprocessing (uses only one CPU).                  |
| `maxcpus=`          | Limits the number of processors used (e.g., `maxcpus=2`).                |
| `isolcpus=`         | Isolates CPUs from the scheduler (used for real-time tasks).             |
| `nohz=`             | Configures the dynamic tick behavior (e.g., `nohz=on` for tickless idle).|
| `highmem=`          | Enables or disables high memory support.                                 |

---

#### 3. **Filesystem Parameters**
| Parameter        | Description                                                                 |
|------------------|-----------------------------------------------------------------------------|
| `rootfstype=`    | Specifies the filesystem type for the root filesystem (e.g., `ext4`).      |
| `fsck.repair=`   | Specifies automatic filesystem repair (`yes`, `preen`, or `no`).          |
| `nfsroot=`       | Specifies the NFS server and path for a root filesystem over NFS.         |
| `resume=`        | Specifies the swap partition used for suspend-to-disk.                   |

---

#### 4. **Network Parameters**
| Parameter        | Description                                                                 |
|------------------|-----------------------------------------------------------------------------|
| `ip=`            | Configures IP settings (e.g., `ip=192.168.1.100::192.168.1.1:255.255.255.0`).|
| `netdev=`        | Specifies a network device to configure (e.g., `netdev=eth0`).            |
| `ipv6.disable=`  | Disables IPv6 (e.g., `ipv6.disable=1`).                                    |

---

#### 5. **Device Parameters**
| Parameter           | Description                                                                 |
|---------------------|-----------------------------------------------------------------------------|
| `acpi=`             | Configures ACPI (e.g., `acpi=off` disables ACPI).                         |
| `irqpoll`           | Forces polling of IRQs (useful for broken hardware).                     |
| `pci=`              | Configures PCI subsystem (e.g., `pci=nomsi` to disable MSI).             |
| `nomodeset`         | Disables kernel mode-setting for graphics.                               |
| `console=`          | Redirects kernel messages to a specific console (e.g., `ttyS0`).         |

---

#### 6. **Debugging and Development Parameters**
| Parameter           | Description                                                                 |
|---------------------|-----------------------------------------------------------------------------|
| `debug`             | Enables kernel debugging messages.                                        |
| `initcall_debug`    | Prints initialization calls for debugging.                                |
| `earlyprintk=`      | Outputs kernel messages early during boot (e.g., `earlyprintk=serial`).  |
| `panic=`            | Sets the timeout in seconds before the kernel reboots after a panic.     |
| `nmi_watchdog=`     | Enables or disables the NMI watchdog.                                    |

---

#### 7. **Power Management Parameters**
| Parameter           | Description                                                                 |
|---------------------|-----------------------------------------------------------------------------|
| `noacpi`            | Disables ACPI.                                                           |
| `apm=`              | Configures APM (e.g., `apm=off` disables Advanced Power Management).     |
| `hibernate=`        | Configures hibernation settings.                                         |
| `idle=`             | Configures CPU idle behavior (e.g., `idle=poll` for no idle states).     |

---

#### 8. **Security Parameters**
| Parameter           | Description                                                                 |
|---------------------|-----------------------------------------------------------------------------|
| `selinux=`          | Enables or disables SELinux (`selinux=0` disables it).                   |
| `apparmor=`         | Configures AppArmor (e.g., `apparmor=0` disables it).                    |
| `securelevel=`      | Sets the system security level.                                           |

---

#### 9. **Timekeeping Parameters**
| Parameter           | Description                                                                 |
|---------------------|-----------------------------------------------------------------------------|
| `clocksource=`      | Specifies the clock source (e.g., `clocksource=tsc`).                    |
| `nohz=`             | Configures tickless behavior.                                             |
| `hpet=`             | Configures the High Precision Event Timer (e.g., `hpet=disable`).        |

---

### **How to View All Available Parameters**
To see a complete list of kernel parameters supported by your specific kernel version:
1. **Boot Documentation**:
   Check the `Documentation/admin-guide/kernel-parameters.txt` file in the Linux source tree:
   ```bash
   less /usr/src/linux/Documentation/admin-guide/kernel-parameters.txt
   ```

2. **Use `sysfs`**:
   View kernel configuration options at runtime:
   ```bash
   cat /proc/cmdline
   ```

3. **Help in Bootloader**:
   GRUB may show some kernel parameter options in its configuration.
