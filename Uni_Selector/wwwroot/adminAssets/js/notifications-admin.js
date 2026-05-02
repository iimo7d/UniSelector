// ===================================
// Notification System - Admin Template Integration
// File: wwwroot/js/notifications-admin.js
// ===================================

class AdminNotificationSystem {
    constructor() {
        this.connection = null;
        this.unreadCount = 0;
        this.isInitialized = false;
    }

    // Initialize the notification system
    async init() {
        if (this.isInitialized) return;

        try {
            // Initialize SignalR connection
            await this.initSignalR();

            // Load initial unread count
            await this.updateUnreadCount();

            // Setup event listeners
            this.setupEventListeners();

            this.isInitialized = true;
            console.log('✅ Admin Notification system initialized');
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

            // Handle incoming notifications
            this.connection.on("ReceiveNotification", (notification) => {
                this.handleNewNotification(notification);
            });

            // Handle unread count updates
            this.connection.on("UnreadCountUpdated", (count) => {
                this.updateBadge(count);
            });

            // Start connection
            await this.connection.start();
            console.log('✅ SignalR connected');

        } catch (error) {
            console.error('❌ SignalR connection error:', error);
            // Retry connection after 5 seconds
            setTimeout(() => this.initSignalR(), 5000);
        }
    }

    // Setup event listeners
    setupEventListeners() {
        // Notification bell click - load notifications
        const notificationBell = document.getElementById('notificationBell');
        if (notificationBell) {
            notificationBell.addEventListener('click', () => this.loadNotifications());
        }

        // View all notifications link
        const viewAllLink = document.getElementById('viewAllNotificationsLink');
        if (viewAllLink) {
            viewAllLink.addEventListener('click', (e) => {
                e.preventDefault();
                window.location.href = '/Notifications';
            });
        }
    }

    // Update unread count from API
    async updateUnreadCount() {
        try {
            const response = await fetch('/api/notifications/count', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
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

        // Update small badge on bell
        const badge = document.getElementById('notificationBadge');
        if (badge) {
            if (count > 0) {
                badge.textContent = count > 99 ? '99+' : count;
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        }

        // Update count badge in dropdown header
        const countBadge = document.getElementById('notificationCountBadge');
        if (countBadge) {
            countBadge.textContent = count > 99 ? '99+' : (count < 10 ? '0' + count : count);
        }

        // Update document title
        this.updateDocumentTitle(count);
    }

    // Update document title with unread count
    updateDocumentTitle(count) {
        const baseTitle = document.title.replace(/^\(\d+\)\s*/, '');
        document.title = count > 0 ? `(${count}) ${baseTitle}` : baseTitle;
    }

    // Load recent notifications into dropdown
    async loadNotifications() {
        try {
            const response = await fetch('/api/notifications/recent?limit=10', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
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

    // Render notifications in dropdown (Admin Template Style)
    renderNotifications(notifications) {
        const container = document.getElementById('notificationsContainer');
        if (!container) return;

        // Hide loading
        const loading = document.getElementById('notificationLoading');
        if (loading) loading.style.display = 'none';

        if (!notifications || notifications.length === 0) {
            container.innerHTML = `
                <div class="text-center py-4">
                    <iconify-icon icon="iconoir:bell-off" class="text-secondary-light" style="font-size: 48px;"></iconify-icon>
                    <p class="text-secondary-light mt-2 mb-0">No notifications</p>
                </div>
            `;
            return;
        }

        container.innerHTML = notifications.map((n, index) =>
            this.createNotificationHTML(n, index)
        ).join('');

        // Add click listeners to each notification
        notifications.forEach(n => {
            const element = document.getElementById(`notification-${n.id}`);
            if (element) {
                element.addEventListener('click', (e) => {
                    // Don't trigger if clicking delete button
                    if (!e.target.closest('.notification-delete-btn')) {
                        this.handleNotificationClick(n);
                    }
                });
            }

            // Delete button listener
            const deleteBtn = document.getElementById(`delete-notification-${n.id}`);
            if (deleteBtn) {
                deleteBtn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    this.deleteNotification(n.id);
                });
            }
        });
    }

    // Create HTML for a single notification (Admin Template Style)
    createNotificationHTML(notification, index) {
        // Alternate background colors like in template
        const bgClass = index % 2 === 1 ? 'bg-neutral-50' : '';
        const unreadClass = !notification.isRead ? 'notification-unread' : '';

        const iconConfig = this.getCategoryIcon(notification.category);
        const avatarHtml = this.getAvatarHtml(iconConfig);

        return `
            <a href="javascript:void(0)" 
               id="notification-${notification.id}"
               class="notification-item px-24 py-12 d-flex align-items-start gap-3 mb-2 justify-content-between ${bgClass} ${unreadClass}"
               style="cursor: pointer; position: relative;">
                <div class="text-black hover-bg-transparent hover-text-primary d-flex align-items-center gap-3">
                    ${avatarHtml}
                    <div>
                        <h6 class="text-md fw-semibold mb-4 ${!notification.isRead ? 'text-primary-600' : ''}">
                            ${this.escapeHtml(notification.title)}
                        </h6>
                        <p class="mb-0 text-sm text-secondary-light text-w-200-px">
                            ${this.escapeHtml(notification.message)}
                        </p>
                    </div>
                </div>
                <div class="d-flex flex-column align-items-end gap-2">
                    <span class="text-sm text-secondary-light flex-shrink-0">${notification.timeAgo}</span>
                    <button class="btn btn-sm btn-link text-danger p-0 notification-delete-btn" 
                            id="delete-notification-${notification.id}"
                            title="Delete"
                            style="font-size: 16px;">
                        <iconify-icon icon="iconoir:trash"></iconify-icon>
                    </button>
                </div>
            </a>
        `;
    }

    // Get avatar HTML based on category
    getAvatarHtml(iconConfig) {
        return `
            <span class="w-44-px h-44-px ${iconConfig.bgClass} ${iconConfig.textClass} rounded-circle d-flex justify-content-center align-items-center flex-shrink-0">
                <iconify-icon icon="${iconConfig.icon}" class="icon text-xxl"></iconify-icon>
            </span>
        `;
    }

    // Get icon configuration based on notification category
    getCategoryIcon(category) {
        const configs = {
            'NewRecommendation': {
                icon: 'iconoir:star',
                bgClass: 'bg-info-subtle',
                textClass: 'text-info-main'
            },
            'ApplicationSubmitted': {
                icon: 'iconoir:page-edit',
                bgClass: 'bg-warning-subtle',
                textClass: 'text-warning-main'
            },
            'ApplicationApproved': {
                icon: 'bitcoin-icons:verify-outline',
                bgClass: 'bg-success-subtle',
                textClass: 'text-success-main'
            },
            'ApplicationRejected': {
                icon: 'iconoir:cancel',
                bgClass: 'bg-danger-subtle',
                textClass: 'text-danger-main'
            },
            'PromoCodeGenerated': {
                icon: 'iconoir:gift',
                bgClass: 'bg-purple-subtle',
                textClass: 'text-purple-main'
            },
            'SystemAlert': {
                icon: 'iconoir:warning-triangle',
                bgClass: 'bg-danger-subtle',
                textClass: 'text-danger-main'
            },
            'CommissionGenerated': {
                icon: 'iconoir:wallet',
                bgClass: 'bg-success-subtle',
                textClass: 'text-success-main'
            },
            'DiscountApplied': {
                icon: 'iconoir:percentage',
                bgClass: 'bg-info-subtle',
                textClass: 'text-info-main'
            }
        };

        return configs[category] || {
            icon: 'iconoir:bell',
            bgClass: 'bg-primary-subtle',
            textClass: 'text-primary-main'
        };
    }

    // Handle notification click
    async handleNotificationClick(notification) {
        // Mark as read
        if (!notification.isRead) {
            await this.markAsRead(notification.id);
        }

        // Navigate to action URL if exists
        if (notification.actionUrl) {
            window.location.href = notification.actionUrl;
        }
    }

    // Handle new notification from SignalR
    handleNewNotification(notification) {
        console.log('📨 New notification received:', notification);

        // Update unread count
        this.updateUnreadCount();

        // Show toast notification
        this.showToast(notification);

        // Play notification sound
        this.playNotificationSound();

        // Reload notifications if dropdown is open
        const dropdown = document.getElementById('notificationDropdown');
        if (dropdown && dropdown.classList.contains('show')) {
            this.loadNotifications();
        }
    }

    // Show toast notification
    showToast(notification) {
        // Check if SweetAlert2 is available
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
        }
    }

    // Play notification sound
    playNotificationSound() {
        try {
            const audio = new Audio('/sounds/notification.mp3');
            audio.volume = 0.3;
            audio.play().catch(e => console.log('Sound play failed:', e));
        } catch (error) {
            // Silently fail if sound doesn't exist
        }
    }

    // Mark single notification as read
    async markAsRead(notificationId) {
        try {
            const response = await fetch(`/api/notifications/${notificationId}/mark-read`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateBadge(data.unreadCount);

                // Update UI - remove unread styling
                const element = document.getElementById(`notification-${notificationId}`);
                if (element) {
                    element.classList.remove('notification-unread');
                    const title = element.querySelector('h6');
                    if (title) title.classList.remove('text-primary-600');
                }
            }
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }

    // Delete notification
    async deleteNotification(notificationId) {
        try {
            const response = await fetch(`/api/notifications/${notificationId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateBadge(data.unreadCount);

                // Remove from UI with animation
                const element = document.getElementById(`notification-${notificationId}`);
                if (element) {
                    element.style.transition = 'opacity 0.3s, transform 0.3s';
                    element.style.opacity = '0';
                    element.style.transform = 'translateX(20px)';

                    setTimeout(() => {
                        element.remove();

                        // Check if container is empty
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
                <div class="text-center py-4">
                    <iconify-icon icon="iconoir:emoji-sad" class="text-danger" style="font-size: 48px;"></iconify-icon>
                    <p class="text-danger mt-2 mb-0">Failed to load notifications</p>
                </div>
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
const adminNotificationSystem = new AdminNotificationSystem();

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    adminNotificationSystem.init();
});

// Reconnect SignalR on page visibility change
document.addEventListener('visibilitychange', () => {
    if (!document.hidden &&
        adminNotificationSystem.connection &&
        adminNotificationSystem.connection.state === signalR.HubConnectionState.Disconnected) {
        adminNotificationSystem.initSignalR();
    }
});