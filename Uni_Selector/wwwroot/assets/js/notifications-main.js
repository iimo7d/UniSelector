
class MainLayoutNotificationSystem {
    constructor() {
        this.connection = null;
        this.unreadCount = 0;
        this.isInitialized = false;
    }

    // Initialize the notification system
    async init() {
        if (this.isInitialized) return;

        try {
            await this.initSignalR();
            await this.updateUnreadCount();
            this.setupEventListeners();
            this.isInitialized = true;
            console.log('✅ Main Layout Notification system initialized');
        } catch (error) {
            console.error('❌ Failed to initialize notification system:', error);
        }
    }

    // Initialize SignalR connection
    async initSignalR() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/notification")
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Information)
                .build();

            this.connection.on("ReceiveNotification", (notification) => {
                this.handleNewNotification(notification);
            });

            this.connection.on("UnreadCountUpdated", (count) => {
                this.updateBadge(count);
            });

            await this.connection.start();
            console.log('✅ SignalR connected');
        } catch (error) {
            console.error('❌ SignalR connection error:', error);
            setTimeout(() => this.initSignalR(), 5000);
        }
    }

    // Setup event listeners
    setupEventListeners() {
        const notificationBell = document.getElementById('notificationBell');
        if (notificationBell) {
            notificationBell.addEventListener('click', (e) => {
                e.preventDefault();
                this.toggleDropdown();
            });
        }

        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            const dropdown = document.getElementById('notificationDropdown');
            const bell = document.getElementById('notificationBell');

            if (dropdown && !dropdown.contains(e.target) && !bell.contains(e.target)) {
                this.closeDropdown();
            }
        });
    }

    // Toggle dropdown visibility (matching cart behavior)
    toggleDropdown() {
        const dropdown = document.getElementById('notificationDropdown');
        if (!dropdown) return;

        const isVisible = dropdown.classList.contains('show');

        if (isVisible) {
            this.closeDropdown();
        } else {
            this.openDropdown();
        }
    }

    // Open dropdown
    openDropdown() {
        const dropdown = document.getElementById('notificationDropdown');
        if (!dropdown) return;

        dropdown.classList.remove('hidden');
        dropdown.classList.add('show');
        dropdown.style.opacity = '1';

        this.loadNotifications();
    }

    // Close dropdown
    closeDropdown() {
        const dropdown = document.getElementById('notificationDropdown');
        if (!dropdown) return;

        dropdown.style.opacity = '0';
        setTimeout(() => {
            dropdown.classList.add('hidden');
            dropdown.classList.remove('show');
        }, 300);
    }

    // Update unread count from API
    async updateUnreadCount() {
        try {
            const response = await fetch('/api/notifications/count', {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateBadge(data.count);
            }
        } catch (error) {
            console.error('Error fetching unread count:', error);
        }
    }

    // Update notification badge
    updateBadge(count) {
        this.unreadCount = count;

        // Update badge on bell icon
        const badge = document.getElementById('notificationBadge');
        if (badge) {
            if (count > 0) {
                badge.textContent = count > 99 ? '99+' : count;
                badge.style.display = 'block';
            } else {
                badge.style.display = 'none';
            }
        }

        // Update unread count in dropdown
        const unreadCountElement = document.getElementById('notificationUnreadCount');
        if (unreadCountElement) {
            unreadCountElement.textContent = count > 99 ? '99+' : count;
        }

        // Update document title
        const baseTitle = document.title.replace(/^\(\d+\)\s*/, '');
        document.title = count > 0 ? `(${count}) ${baseTitle}` : baseTitle;
    }

    // Load recent notifications
    async loadNotifications() {
        try {
            const response = await fetch('/api/notifications/recent?limit=5', {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (response.ok) {
                const data = await response.json();
                this.renderNotifications(data.notifications);
            }
        } catch (error) {
            console.error('Error loading notifications:', error);
            this.showNotificationError();
        }
    }

    // Render notifications (matching cart item structure)
    renderNotifications(notifications) {
        const container = document.getElementById('notificationsContainer');
        if (!container) return;

        // Hide loading
        const loading = document.getElementById('notificationLoading');
        if (loading) loading.style.display = 'none';

        if (!notifications || notifications.length === 0) {
            container.innerHTML = `
                <li class="text-center py-8">
                    <i class="icofont-bell-alt text-5xl text-contentColor opacity-50"></i>
                    <p class="text-sm text-contentColor dark:text-contentColor-dark mt-2">No notifications</p>
                    <p class="text-xs text-contentColor dark:text-contentColor-dark opacity-70">You're all caught up!</p>
                </li>
            `;
            return;
        }

        container.innerHTML = notifications.map(n => this.createNotificationHTML(n)).join('');

        // Add click listeners
        notifications.forEach(n => {
            const element = document.getElementById(`notification-${n.id}`);
            if (element) {
                element.addEventListener('click', (e) => {
                    if (!e.target.closest('.notification-delete-btn')) {
                        this.handleNotificationClick(n);
                    }
                });
            }

            const deleteBtn = document.getElementById(`delete-notification-${n.id}`);
            if (deleteBtn) {
                deleteBtn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    this.deleteNotification(n.id);
                });
            }
        });
    }

    // Create HTML for single notification (matching cart item structure)
    createNotificationHTML(notification) {
        const unreadClass = !notification.isRead ? 'notification-unread' : '';
        const iconConfig = this.getCategoryIcon(notification.category);

        return `
            <li class="relative flex gap-x-15px items-center notification-item ${unreadClass}" 
                id="notification-${notification.id}">
                <!-- Icon (replaces product image) -->
                <div class="flex-shrink-0">
                    <div class="w-16 h-16 rounded-full ${iconConfig.bgClass} flex items-center justify-center">
                        <i class="${iconConfig.icon} text-2xl ${iconConfig.textClass}"></i>
                    </div>
                </div>
                
                <!-- Content (replaces product name and price) -->
                <div>
                    <a href="javascript:void(0)" 
                       class="text-sm ${!notification.isRead ? 'font-bold' : ''} text-darkblack hover:text-secondaryColor leading-5 block pb-2 capitalize dark:text-darkblack-dark dark:hover:text-secondaryColor">
                        ${this.escapeHtml(notification.title)}
                    </a>
                    <p class="text-sm text-contentColor leading-5 block pb-5px dark:text-contentColor-dark truncate-2-lines">
                        ${this.escapeHtml(notification.message)}
                    </p>
                    <p class="text-xs text-contentColor dark:text-contentColor-dark opacity-70">
                        <i class="icofont-clock-time"></i> ${notification.timeAgo}
                    </p>
                </div>

                <!-- Delete button (replaces remove from cart button) -->
                <button class="absolute block top-0 right-0 text-base text-contentColor leading-1 hover:text-secondaryColor dark:text-contentColor-dark dark:hover:text-secondaryColor notification-delete-btn" 
                        id="delete-notification-${notification.id}"
                        title="Delete notification">
                    <i class="icofont-close-line"></i>
                </button>
            </li>
        `;
    }

    // Get icon configuration based on notification category
    getCategoryIcon(category) {
        const configs = {
            'NewRecommendation': {
                icon: 'icofont-star',
                bgClass: 'bg-blue-100',
                textClass: 'text-blue-600'
            },
            'ApplicationSubmitted': {
                icon: 'icofont-paper',
                bgClass: 'bg-yellow-100',
                textClass: 'text-yellow-600'
            },
            'ApplicationApproved': {
                icon: 'icofont-check-circled',
                bgClass: 'bg-green-100',
                textClass: 'text-green-600'
            },
            'ApplicationRejected': {
                icon: 'icofont-close-circled',
                bgClass: 'bg-red-100',
                textClass: 'text-red-600'
            },
            'PromoCodeGenerated': {
                icon: 'icofont-gift',
                bgClass: 'bg-purple-100',
                textClass: 'text-purple-600'
            },
            'SystemAlert': {
                icon: 'icofont-warning',
                bgClass: 'bg-red-100',
                textClass: 'text-red-600'
            },
            'CommissionGenerated': {
                icon: 'icofont-wallet',
                bgClass: 'bg-green-100',
                textClass: 'text-green-600'
            },
            'DiscountApplied': {
                icon: 'icofont-sale-discount',
                bgClass: 'bg-orange-100',
                textClass: 'text-orange-600'
            }
        };

        return configs[category] || {
            icon: 'icofont-bell',
            bgClass: 'bg-gray-100',
            textClass: 'text-gray-600'
        };
    }

    // Handle notification click
    async handleNotificationClick(notification) {
        if (!notification.isRead) {
            await this.markAsRead(notification.id);
        }

        this.closeDropdown();

        if (notification.actionUrl) {
            window.location.href = notification.actionUrl;
        }
    }

    // Handle new notification from SignalR
    handleNewNotification(notification) {
        console.log('📨 New notification received:', notification);

        this.updateUnreadCount();
        this.showToast(notification);
        this.playNotificationSound();

        const dropdown = document.getElementById('notificationDropdown');
        if (dropdown && dropdown.classList.contains('show')) {
            this.loadNotifications();
        }
    }

    // Show toast notification
    showToast(notification) {
        if (typeof Swal !== 'undefined') {
            const toast = Swal.mixin({
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 5000,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.addEventListener('mouseenter', Swal.stopTimer);
                    toast.addEventListener('mouseleave', Swal.resumeTimer);
                }
            });

            toast.fire({
                icon: 'info',
                title: notification.title,
                text: notification.message
            });
        } else {
            this.showSimpleNotification(notification);
        }
    }

    // Simple notification fallback
    showSimpleNotification(notification) {
        const banner = document.createElement('div');
        banner.className = 'fixed top-4 right-4 bg-white dark:bg-whiteColor-dark shadow-lg rounded-lg p-4 max-w-sm z-50 animate-slideIn';
        banner.innerHTML = `
            <div class="flex items-start gap-3">
                <i class="icofont-bell text-secondaryColor text-xl"></i>
                <div class="flex-1">
                    <h4 class="font-bold text-sm text-darkblack dark:text-darkblack-dark">${this.escapeHtml(notification.title)}</h4>
                    <p class="text-xs text-contentColor dark:text-contentColor-dark mt-1">${this.escapeHtml(notification.message)}</p>
                </div>
                <button onclick="this.parentElement.parentElement.remove()" class="text-contentColor hover:text-secondaryColor">
                    <i class="icofont-close-line"></i>
                </button>
            </div>
        `;
        document.body.appendChild(banner);
        setTimeout(() => banner.remove(), 5000);
    }

    // Play notification sound
    playNotificationSound() {
        try {
            const audio = new Audio('/sounds/notification.mp3');
            audio.volume = 0.3;
            audio.play().catch(e => console.log('Sound play failed:', e));
        } catch (error) {
            // Silently fail
        }
    }

    // Mark single notification as read
    async markAsRead(notificationId) {
        try {
            const response = await fetch(`/api/notifications/${notificationId}/mark-read`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateBadge(data.unreadCount);

                const element = document.getElementById(`notification-${notificationId}`);
                if (element) {
                    element.classList.remove('notification-unread');
                    const title = element.querySelector('a');
                    if (title) title.classList.remove('font-bold');
                }
            }
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }

    // Mark all notifications as read
    async markAllAsRead() {
        try {
            const response = await fetch('/api/notifications/mark-all-read', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateBadge(data.unreadCount);
                await this.loadNotifications();

                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'All notifications marked as read',
                        timer: 2000,
                        showConfirmButton: false
                    });
                }
            }
        } catch (error) {
            console.error('Error marking all as read:', error);
        }
    }

    // Delete notification
    async deleteNotification(notificationId) {
        try {
            const response = await fetch(`/api/notifications/${notificationId}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateBadge(data.unreadCount);

                const element = document.getElementById(`notification-${notificationId}`);
                if (element) {
                    element.style.transition = 'opacity 0.3s, transform 0.3s';
                    element.style.opacity = '0';
                    element.style.transform = 'translateX(20px)';

                    setTimeout(() => {
                        element.remove();

                        const container = document.getElementById('notificationsContainer');
                        if (container && container.children.length === 0) {
                            this.loadNotifications();
                        }
                    }, 300);
                }
            }
        } catch (error) {
            console.error('Error deleting notification:', error);
        }
    }

    // Show error message
    showNotificationError() {
        const container = document.getElementById('notificationsContainer');
        if (container) {
            container.innerHTML = `
                <li class="text-center py-8">
                    <i class="icofont-sad text-5xl text-red-500"></i>
                    <p class="text-sm text-red-500 mt-2">Failed to load notifications</p>
                </li>
            `;
        }
    }

    // Escape HTML to prevent XSS
    escapeHtml(text) {
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, m => map[m]);
    }
}

// Create global instance
const notificationSystem = new MainLayoutNotificationSystem();

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    notificationSystem.init();
});

// Reconnect SignalR on page visibility change
document.addEventListener('visibilitychange', () => {
    if (!document.hidden &&
        notificationSystem.connection &&
        notificationSystem.connection.state === signalR.HubConnectionState.Disconnected) {
        notificationSystem.initSignalR();
    }
});

// Add animation styles
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    .animate-slideIn {
        animation: slideIn 0.3s ease-out;
    }
`;
document.head.appendChild(style);
