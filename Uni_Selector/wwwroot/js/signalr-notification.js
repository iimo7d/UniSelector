
(function () {
    'use strict';

    const notificationManager = {
        connection: null,
        isConnected: false,

        init: function () {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/notificationHub")
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        if (retryContext.elapsedMilliseconds < 60000) {
                            return Math.random() * 10000;
                        } else {
                            return null;
                        }
                    }
                })
                .configureLogging(signalR.LogLevel.Information)
                .build();

            this.setupEventHandlers();
            this.start();
        },

        setupEventHandlers: function () {
            const self = this;

            this.connection.on("ReceiveNotification", function (notification) {
                self.showNotification(notification);
                self.playNotificationSound();
            });

            this.connection.on("UnreadCountUpdated", function (count) {
                self.updateUnreadCount(count);
            });

            this.connection.on("NotificationMarkedAsRead", function (notificationId) {
                self.markNotificationAsReadInUI(notificationId);
            });

            this.connection.on("AllNotificationsMarkedAsRead", function () {
                self.markAllNotificationsAsReadInUI();
            });

            this.connection.on("RecentNotifications", function (notifications) {
                self.displayRecentNotifications(notifications);
            });

            this.connection.on("Error", function (errorMessage) {
                console.error("SignalR Error:", errorMessage);
                self.showErrorToast(errorMessage);
            });

            this.connection.onreconnecting((error) => {
                console.log("SignalR reconnecting...", error);
                self.isConnected = false;
                self.showReconnectingStatus();
            });

            this.connection.onreconnected((connectionId) => {
                console.log("SignalR reconnected:", connectionId);
                self.isConnected = true;
                self.hideReconnectingStatus();
                self.getUnreadCount();
            });

            this.connection.onclose((error) => {
                console.log("SignalR connection closed:", error);
                self.isConnected = false;
                self.showDisconnectedStatus();
                setTimeout(() => self.start(), 5000);
            });
        },

        start: async function () {
            try {
                await this.connection.start();
                console.log("SignalR Connected");
                this.isConnected = true;
                this.hideDisconnectedStatus();
                this.getUnreadCount();
                this.subscribeToUniversityGroupIfNeeded();
            } catch (err) {
                console.error("SignalR Connection Error:", err);
                setTimeout(() => this.start(), 5000);
            }
        },

        getUnreadCount: function () {
            if (this.isConnected) {
                this.connection.invoke("GetUnreadCount").catch(err => console.error(err));
            }
        },

        getRecentNotifications: function (count = 10) {
            if (this.isConnected) {
                this.connection.invoke("GetRecentNotifications", count).catch(err => console.error(err));
            }
        },

        markNotificationAsRead: function (notificationId) {
            if (this.isConnected) {
                this.connection.invoke("MarkNotificationAsRead", notificationId).catch(err => console.error(err));
            }
        },

        markAllNotificationsAsRead: function () {
            if (this.isConnected) {
                this.connection.invoke("MarkAllNotificationsAsRead").catch(err => console.error(err));
            }
        },

        joinUniversityGroup: function (universityId) {
            if (this.isConnected) {
                this.connection.invoke("JoinUniversityGroup", universityId).catch(err => console.error(err));
            }
        },

        leaveUniversityGroup: function (universityId) {
            if (this.isConnected) {
                this.connection.invoke("LeaveUniversityGroup", universityId).catch(err => console.error(err));
            }
        },

        showNotification: function (notification) {
            const notificationHtml = `
                <div class="notification-item ${notification.type}" data-id="${notification.id || ''}">
                    <div class="notification-header">
                        <strong>${this.escapeHtml(notification.title)}</strong>
                        <span class="notification-time">${this.formatTime(notification.timestamp)}</span>
                    </div>
                    <div class="notification-body">
                        ${this.escapeHtml(notification.message)}
                    </div>
                    ${notification.actionUrl ? `<a href="${notification.actionUrl}" class="notification-action">View Details</a>` : ''}
                </div>
            `;

            const notificationContainer = document.getElementById('notification-toast-container');
            if (notificationContainer) {
                const div = document.createElement('div');
                div.innerHTML = notificationHtml;
                notificationContainer.appendChild(div.firstElementChild);

                setTimeout(() => {
                    div.firstElementChild.classList.add('fade-out');
                    setTimeout(() => div.firstElementChild.remove(), 300);
                }, 5000);
            }

            this.appendToNotificationList(notification);
        },

        updateUnreadCount: function (count) {
            const badges = document.querySelectorAll('.notification-badge, .unread-count');
            badges.forEach(badge => {
                badge.textContent = count;
                badge.style.display = count > 0 ? 'inline-block' : 'none';
            });
        },

        markNotificationAsReadInUI: function (notificationId) {
            const notificationElement = document.querySelector(`[data-notification-id="${notificationId}"]`);
            if (notificationElement) {
                notificationElement.classList.remove('unread');
                notificationElement.classList.add('read');
            }
        },

        markAllNotificationsAsReadInUI: function () {
            const notificationElements = document.querySelectorAll('.notification-item.unread');
            notificationElements.forEach(element => {
                element.classList.remove('unread');
                element.classList.add('read');
            });
        },

        displayRecentNotifications: function (notifications) {
            const container = document.getElementById('recent-notifications-container');
            if (!container) return;

            container.innerHTML = '';

            if (notifications.length === 0) {
                container.innerHTML = '<div class="no-notifications">No notifications</div>';
                return;
            }

            notifications.forEach(notification => {
                const notificationHtml = `
                    <div class="notification-item ${notification.IsRead ? 'read' : 'unread'}" 
                         data-notification-id="${notification.Id}">
                        <div class="notification-header">
                            <strong>${this.escapeHtml(notification.Title)}</strong>
                            <span class="notification-time">${this.formatDate(notification.CreatedAt)}</span>
                        </div>
                        <div class="notification-body">
                            ${this.escapeHtml(notification.Message)}
                        </div>
                        <div class="notification-actions">
                            ${notification.ActionUrl ? `<a href="${notification.ActionUrl}" class="btn-view">View</a>` : ''}
                            ${!notification.IsRead ? `<button class="btn-mark-read" onclick="notificationManager.markNotificationAsRead(${notification.Id})">Mark as Read</button>` : ''}
                        </div>
                    </div>
                `;
                container.innerHTML += notificationHtml;
            });
        },

        appendToNotificationList: function (notification) {
            const listContainer = document.getElementById('notification-list');
            if (!listContainer) return;

            const notificationHtml = `
                <div class="notification-item unread" data-notification-id="${notification.id}">
                    <div class="notification-header">
                        <strong>${this.escapeHtml(notification.title)}</strong>
                        <span class="notification-time">${this.formatTime(notification.timestamp)}</span>
                    </div>
                    <div class="notification-body">
                        ${this.escapeHtml(notification.message)}
                    </div>
                    <div class="notification-actions">
                        ${notification.actionUrl ? `<a href="${notification.actionUrl}" class="btn-view">View</a>` : ''}
                        <button class="btn-mark-read" onclick="notificationManager.markNotificationAsRead(${notification.id})">Mark as Read</button>
                    </div>
                </div>
            `;

            listContainer.insertAdjacentHTML('afterbegin', notificationHtml);
        },

        playNotificationSound: function () {
            const audio = document.getElementById('notification-sound');
            if (audio) {
                audio.play().catch(e => console.log('Could not play notification sound:', e));
            }
        },

        showErrorToast: function (message) {
            alert(message);
        },

        showReconnectingStatus: function () {
            const statusElement = document.getElementById('connection-status');
            if (statusElement) {
                statusElement.textContent = 'Reconnecting...';
                statusElement.className = 'status-reconnecting';
            }
        },

        hideReconnectingStatus: function () {
            const statusElement = document.getElementById('connection-status');
            if (statusElement) {
                statusElement.textContent = 'Connected';
                statusElement.className = 'status-connected';
                setTimeout(() => statusElement.style.display = 'none', 2000);
            }
        },

        showDisconnectedStatus: function () {
            const statusElement = document.getElementById('connection-status');
            if (statusElement) {
                statusElement.textContent = 'Disconnected';
                statusElement.className = 'status-disconnected';
                statusElement.style.display = 'block';
            }
        },

        hideDisconnectedStatus: function () {
            const statusElement = document.getElementById('connection-status');
            if (statusElement) {
                statusElement.style.display = 'none';
            }
        },

        subscribeToUniversityGroupIfNeeded: function () {
            const universityId = document.body.getAttribute('data-university-id');
            if (universityId) {
                this.joinUniversityGroup(parseInt(universityId));
            }
        },

        escapeHtml: function (text) {
            const map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, m => map[m]);
        },

        formatTime: function (timestamp) {
            const date = new Date(timestamp);
            const now = new Date();
            const diffMs = now - date;
            const diffMins = Math.floor(diffMs / 60000);

            if (diffMins < 1) return 'Just now';
            if (diffMins < 60) return `${diffMins}m ago`;

            const diffHours = Math.floor(diffMins / 60);
            if (diffHours < 24) return `${diffHours}h ago`;

            const diffDays = Math.floor(diffHours / 24);
            if (diffDays < 7) return `${diffDays}d ago`;

            return date.toLocaleDateString();
        },

        formatDate: function (dateString) {
            const date = new Date(dateString);
            return date.toLocaleString();
        }
    };

    window.notificationManager = notificationManager;

    document.addEventListener('DOMContentLoaded', function () {
        if (document.body.hasAttribute('data-user-authenticated')) {
            notificationManager.init();
        }
    });

    document.getElementById('mark-all-read-btn')?.addEventListener('click', function () {
        notificationManager.markAllNotificationsAsRead();
    });

    document.getElementById('refresh-notifications-btn')?.addEventListener('click', function () {
        notificationManager.getRecentNotifications();
    });
})();
