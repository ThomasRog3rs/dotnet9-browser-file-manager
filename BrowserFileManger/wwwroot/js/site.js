// SoundVault - UI Enhancement Scripts

(function() {
    'use strict';

    // ========================================
    // Drag and Drop Upload Enhancement
    // ========================================
    
    const uploadZone = document.getElementById('uploadZone');
    const fileInput = document.getElementById('fileInput');
    const uploadBtn = document.getElementById('uploadBtn');
    const uploadForm = document.getElementById('uploadForm');
    
    if (uploadZone && fileInput) {
        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            uploadZone.addEventListener(eventName, preventDefaults, false);
            document.body.addEventListener(eventName, preventDefaults, false);
        });

        function preventDefaults(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        // Highlight drop zone when item is dragged over it
        ['dragenter', 'dragover'].forEach(eventName => {
            uploadZone.addEventListener(eventName, highlight, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            uploadZone.addEventListener(eventName, unhighlight, false);
        });

        function highlight(e) {
            uploadZone.classList.add('drag-over');
        }

        function unhighlight(e) {
            uploadZone.classList.remove('drag-over');
        }

        // Handle dropped files
        uploadZone.addEventListener('drop', handleDrop, false);

        function handleDrop(e) {
            const dt = e.dataTransfer;
            const files = dt.files;

            if (files.length > 0) {
                // Transfer files to the file input
                fileInput.files = files;
                updateFileDisplay(files[0]);
            }
        }

        // Update display when file is selected
        fileInput.addEventListener('change', function(e) {
            if (this.files.length > 0) {
                updateFileDisplay(this.files[0]);
            }
        });

        function updateFileDisplay(file) {
            const uploadTitle = uploadZone.querySelector('.upload-title');
            const uploadSubtitle = uploadZone.querySelector('.upload-subtitle');
            
            if (uploadTitle && uploadSubtitle) {
                uploadTitle.textContent = file.name;
                uploadSubtitle.textContent = formatFileSize(file.size);
                uploadTitle.style.color = 'var(--color-accent-primary)';
            }
            
            // Update button text
            if (uploadBtn) {
                uploadBtn.innerHTML = `
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <polyline points="20 6 9 17 4 12"></polyline>
                    </svg>
                    Upload "${truncateFilename(file.name, 20)}"
                `;
            }
        }

        function formatFileSize(bytes) {
            if (bytes === 0) return '0 Bytes';
            const k = 1024;
            const sizes = ['Bytes', 'KB', 'MB', 'GB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
        }

        function truncateFilename(filename, maxLength) {
            if (filename.length <= maxLength) return filename;
            const ext = filename.split('.').pop();
            const name = filename.substring(0, filename.length - ext.length - 1);
            const truncatedName = name.substring(0, maxLength - ext.length - 4) + '...';
            return truncatedName + '.' + ext;
        }
    }

    // ========================================
    // Form Submit Enhancement
    // ========================================
    
    if (uploadForm && uploadBtn) {
        uploadForm.addEventListener('submit', function(e) {
            if (fileInput && fileInput.files.length > 0) {
                uploadBtn.disabled = true;
                uploadBtn.innerHTML = `
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="spin">
                        <line x1="12" y1="2" x2="12" y2="6"></line>
                        <line x1="12" y1="18" x2="12" y2="22"></line>
                        <line x1="4.93" y1="4.93" x2="7.76" y2="7.76"></line>
                        <line x1="16.24" y1="16.24" x2="19.07" y2="19.07"></line>
                        <line x1="2" y1="12" x2="6" y2="12"></line>
                        <line x1="18" y1="12" x2="22" y2="12"></line>
                        <line x1="4.93" y1="19.07" x2="7.76" y2="16.24"></line>
                        <line x1="16.24" y1="7.76" x2="19.07" y2="4.93"></line>
                    </svg>
                    Uploading...
                `;
                
                // Add spinning animation
                const style = document.createElement('style');
                style.textContent = `
                    @keyframes spin {
                        from { transform: rotate(0deg); }
                        to { transform: rotate(360deg); }
                    }
                    .spin {
                        animation: spin 1s linear infinite;
                    }
                `;
                document.head.appendChild(style);
            }
        });
    }

    // ========================================
    // Smooth Scroll to Library After Load
    // ========================================
    
    // Check if we just uploaded a file (URL contains hash or referrer check)
    if (window.location.hash === '#library') {
        const librarySection = document.querySelector('.library-section');
        if (librarySection) {
            setTimeout(() => {
                librarySection.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }, 100);
        }
    }

    // ========================================
    // Audio Player Enhancements
    // ========================================
    
    const audioPlayers = document.querySelectorAll('.audio-player');
    
    audioPlayers.forEach(player => {
        // Pause other players when one starts
        player.addEventListener('play', function() {
            audioPlayers.forEach(otherPlayer => {
                if (otherPlayer !== player) {
                    otherPlayer.pause();
                }
            });
        });
    });

    // Autoplay the next track within explicitly marked lists
    const autoplayLists = document.querySelectorAll('[data-autoplay-next="true"]');

    autoplayLists.forEach(list => {
        const players = Array.from(list.querySelectorAll('.audio-player'));

        players.forEach((player, index) => {
            player.addEventListener('ended', function() {
                const nextPlayer = players[index + 1];
                if (!nextPlayer) return;

                const playPromise = nextPlayer.play();
                if (playPromise && typeof playPromise.catch === 'function') {
                    playPromise.catch(() => {
                        // Autoplay may be blocked; ignore to avoid console noise.
                    });
                }
            });
        });
    });

    // ========================================
    // Card Hover Sound Effect (Optional)
    // ========================================
    
    // Subtle visual feedback on track cards
    const trackCards = document.querySelectorAll('.track-card');
    
    trackCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.zIndex = '10';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.zIndex = '1';
        });
    });

    // ========================================
    // Keyboard Navigation
    // ========================================
    
    document.addEventListener('keydown', function(e) {
        // Space to toggle current audio
        if (e.code === 'Space' && e.target.tagName !== 'INPUT') {
            const playingAudio = document.querySelector('.audio-player:not([paused])');
            if (playingAudio) {
                e.preventDefault();
                if (playingAudio.paused) {
                    playingAudio.play();
                } else {
                    playingAudio.pause();
                }
            }
        }
    });

})();
