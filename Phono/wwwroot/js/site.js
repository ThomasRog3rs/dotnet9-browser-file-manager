// Phono - UI Enhancement Scripts

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
    // Unified Music Player
    // ========================================
    
    const unifiedPlayer = document.getElementById('unifiedPlayer');
    const trackList = document.getElementById('trackList');
    
    if (unifiedPlayer && trackList) {
        const audioElement = document.getElementById('audioElement');
        const playerArtwork = document.getElementById('playerArtwork');
        const playerTitle = document.getElementById('playerTitle');
        const playerArtist = document.getElementById('playerArtist');
        const playerPlayPause = document.getElementById('playerPlayPause');
        const playerPrev = document.getElementById('playerPrev');
        const playerNext = document.getElementById('playerNext');
        const playerSeek = document.getElementById('playerSeek');
        const playerCurrentTime = document.getElementById('playerCurrentTime');
        const playerTotalTime = document.getElementById('playerTotalTime');
        const playerVolume = document.getElementById('playerVolume');
        const playerVolumeBtn = document.getElementById('playerVolumeBtn');
        const playAllBtn = document.getElementById('playAllBtn');
        
        const playIcon = playerPlayPause.querySelector('.play-icon');
        const pauseIcon = playerPlayPause.querySelector('.pause-icon');
        const volumeOn = playerVolumeBtn.querySelector('.volume-on');
        const volumeOff = playerVolumeBtn.querySelector('.volume-off');
        
        // Playlist state
        let playlist = [];
        let currentIndex = -1;
        let previousVolume = 0.8;
        
        // Build playlist from track rows
        function buildPlaylist() {
            const rows = trackList.querySelectorAll('.track-list-row');
            playlist = Array.from(rows).map((row, index) => ({
                id: row.dataset.trackId,
                file: row.dataset.trackFile,
                title: row.dataset.trackTitle,
                artist: row.dataset.trackArtist,
                album: row.dataset.trackAlbum,
                artwork: row.dataset.trackArtwork,
                mime: row.dataset.trackMime,
                element: row,
                index: index
            }));
        }
        
        // Update player UI
        function updatePlayerUI(track) {
            playerTitle.textContent = track.title || 'Unknown Track';
            playerArtist.textContent = track.artist || 'Unknown Artist';
            
            // Update artwork
            if (track.artwork) {
                playerArtwork.innerHTML = `<img src="${track.artwork}" alt="${track.title}" />`;
            } else {
                playerArtwork.innerHTML = `
                    <div class="player-artwork-placeholder">
                        <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                            <circle cx="12" cy="12" r="10"></circle>
                            <circle cx="12" cy="12" r="3"></circle>
                        </svg>
                    </div>
                `;
            }
            
            // Update active row styling
            playlist.forEach(t => t.element.classList.remove('is-playing'));
            track.element.classList.add('is-playing');
        }
        
        // Play a track by index
        function playTrack(index) {
            if (index < 0 || index >= playlist.length) return;
            
            const track = playlist[index];
            if (!track.mime) {
                // Skip unsupported formats
                if (index < playlist.length - 1) {
                    playTrack(index + 1);
                }
                return;
            }
            
            currentIndex = index;
            audioElement.src = `/uploads/${track.file}`;
            audioElement.type = track.mime;
            
            updatePlayerUI(track);
            unifiedPlayer.classList.add('is-visible');
            
            const playPromise = audioElement.play();
            if (playPromise) {
                playPromise.catch(() => {
                    // Autoplay blocked - user needs to interact
                });
            }
        }
        
        // Toggle play/pause
        function togglePlayPause() {
            if (currentIndex === -1 && playlist.length > 0) {
                playTrack(0);
                return;
            }
            
            if (audioElement.paused) {
                audioElement.play();
            } else {
                audioElement.pause();
            }
        }
        
        // Play previous track
        function playPrevious() {
            if (currentIndex > 0) {
                playTrack(currentIndex - 1);
            } else if (playlist.length > 0) {
                playTrack(playlist.length - 1);
            }
        }
        
        // Play next track
        function playNext() {
            if (currentIndex < playlist.length - 1) {
                playTrack(currentIndex + 1);
            } else if (playlist.length > 0) {
                playTrack(0);
            }
        }
        
        // Format time display
        function formatTime(seconds) {
            if (isNaN(seconds) || !isFinite(seconds)) return '0:00';
            const mins = Math.floor(seconds / 60);
            const secs = Math.floor(seconds % 60);
            return `${mins}:${secs.toString().padStart(2, '0')}`;
        }
        
        // Update play/pause icons
        function updatePlayPauseIcons(isPlaying) {
            if (isPlaying) {
                playIcon.style.display = 'none';
                pauseIcon.style.display = 'block';
            } else {
                playIcon.style.display = 'block';
                pauseIcon.style.display = 'none';
            }
        }
        
        // Update volume icons
        function updateVolumeIcons(isMuted) {
            if (isMuted) {
                volumeOn.style.display = 'none';
                volumeOff.style.display = 'block';
            } else {
                volumeOn.style.display = 'block';
                volumeOff.style.display = 'none';
            }
        }
        
        // Event listeners
        playerPlayPause.addEventListener('click', togglePlayPause);
        playerPrev.addEventListener('click', playPrevious);
        playerNext.addEventListener('click', playNext);
        
        // Track row clicks
        trackList.addEventListener('click', function(e) {
            const row = e.target.closest('.track-list-row');
            if (row && !e.target.closest('.track-list-actions')) {
                const index = playlist.findIndex(t => t.id === row.dataset.trackId);
                if (index !== -1) {
                    playTrack(index);
                }
            }
        });
        
        // Play all button
        if (playAllBtn) {
            playAllBtn.addEventListener('click', function() {
                if (playlist.length > 0) {
                    playTrack(0);
                }
            });
        }
        
        // Audio element events
        audioElement.addEventListener('play', function() {
            updatePlayPauseIcons(true);
        });
        
        audioElement.addEventListener('pause', function() {
            updatePlayPauseIcons(false);
        });
        
        audioElement.addEventListener('ended', function() {
            playNext();
        });
        
        audioElement.addEventListener('timeupdate', function() {
            const current = audioElement.currentTime;
            const duration = audioElement.duration;
            
            playerCurrentTime.textContent = formatTime(current);
            
            if (duration && isFinite(duration)) {
                playerSeek.value = (current / duration) * 100;
            }
        });
        
        audioElement.addEventListener('loadedmetadata', function() {
            playerTotalTime.textContent = formatTime(audioElement.duration);
        });
        
        // Seek bar
        playerSeek.addEventListener('input', function() {
            const duration = audioElement.duration;
            if (duration && isFinite(duration)) {
                audioElement.currentTime = (this.value / 100) * duration;
            }
        });
        
        // Volume control
        playerVolume.addEventListener('input', function() {
            const volume = this.value / 100;
            audioElement.volume = volume;
            updateVolumeIcons(volume === 0);
        });
        
        // Mute toggle
        playerVolumeBtn.addEventListener('click', function() {
            if (audioElement.volume > 0) {
                previousVolume = audioElement.volume;
                audioElement.volume = 0;
                playerVolume.value = 0;
                updateVolumeIcons(true);
            } else {
                audioElement.volume = previousVolume;
                playerVolume.value = previousVolume * 100;
                updateVolumeIcons(false);
            }
        });
        
        // Initialize
        buildPlaylist();
        audioElement.volume = playerVolume.value / 100;
        
        // Show player if we have tracks
        if (playlist.length > 0) {
            // Player starts hidden, will show when user clicks a track
        }
    }
    
    // ========================================
    // Keyboard Navigation (Updated for Unified Player)
    // ========================================
    
    document.addEventListener('keydown', function(e) {
        const audioElement = document.getElementById('audioElement');
        
        // Space to toggle play/pause
        if (e.code === 'Space' && e.target.tagName !== 'INPUT' && e.target.tagName !== 'TEXTAREA') {
            if (audioElement && audioElement.src) {
                e.preventDefault();
                if (audioElement.paused) {
                    audioElement.play();
                } else {
                    audioElement.pause();
                }
            } else {
                // Fallback to old card-based players
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
        }
        
        // Arrow keys for prev/next (when not in input)
        if (e.target.tagName !== 'INPUT' && e.target.tagName !== 'TEXTAREA') {
            const playerPrev = document.getElementById('playerPrev');
            const playerNext = document.getElementById('playerNext');
            
            if (e.code === 'ArrowLeft' && playerPrev) {
                e.preventDefault();
                playerPrev.click();
            }
            if (e.code === 'ArrowRight' && playerNext) {
                e.preventDefault();
                playerNext.click();
            }
        }
    });

})();
