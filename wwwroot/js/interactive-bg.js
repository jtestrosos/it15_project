window.initInteractiveBg = function(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    
    // Resize handling
    let width = window.innerWidth;
    let height = window.innerHeight;
    
    const resize = () => {
        width = window.innerWidth;
        height = window.innerHeight;
        canvas.width = width;
        canvas.height = height;
    };
    
    window.addEventListener('resize', resize);
    resize();
    
    // Mouse properties
    const mouse = {
        x: null,
        y: null,
        radius: 120
    };
    
    window.addEventListener('mousemove', function(event) {
        mouse.x = event.x;
        mouse.y = event.y;
    });
    
    // Particles
    const particles = [];
    const color = 'rgba(76, 29, 149, 0.7)'; // dark rich purple
    
    const density = 6000; 
    let particleCount = Math.floor((width * height) / density);
    if(particleCount > 250) particleCount = 250;
    else if (particleCount < 50) particleCount = 50;

    class Particle {
        constructor() {
            this.x = Math.random() * width;
            this.y = Math.random() * height;
            this.size = Math.random() * 2 + 1;
            this.baseX = this.x;
            this.baseY = this.y;
            this.density = (Math.random() * 30) + 1;
            // Dramatically slow down passive movement
            this.vx = (Math.random() - 0.5) * 0.4;
            this.vy = (Math.random() - 0.5) * 0.4;
        }
        
        draw() {
            ctx.fillStyle = color;
            ctx.beginPath();
            ctx.arc(this.x, this.y, this.size, 0, Math.PI * 2);
            ctx.closePath();
            ctx.fill();
        }
        
        update() {
            // Random drifting
            this.x += this.vx;
            this.y += this.vy;

            // Bounce off walls
            if (this.x < 0 || this.x > width) this.vx *= -1;
            if (this.y < 0 || this.y > height) this.vy *= -1;

            // Interactive mouse repelling
            if (mouse.x != null && mouse.y != null) {
                let dx = mouse.x - this.x;
                let dy = mouse.y - this.y;
                let distance = Math.sqrt(dx * dx + dy * dy);
                let forceDirectionX = dx / distance;
                let forceDirectionY = dy / distance;
                let maxDistance = mouse.radius;
                let force = (maxDistance - distance) / maxDistance;
                // Soften the mouse interaction density ratio so it doesn't violently throw particles
                let directionX = forceDirectionX * force * (this.density * 0.15);
                let directionY = forceDirectionY * force * (this.density * 0.15);

                if (distance < mouse.radius) {
                    this.x -= directionX;
                    this.y -= directionY;
                }
            }
            this.draw();
        }
    }
    
    function init() {
        particles.length = 0;
        for (let i = 0; i < particleCount; i++) {
            particles.push(new Particle());
        }
    }
    
    init();
    
    let animationFrameId;

    function animate() {
        ctx.clearRect(0, 0, width, height);
        for (let i = 0; i < particles.length; i++) {
            particles[i].update();
        }
        connect();
        animationFrameId = requestAnimationFrame(animate);
    }
    
    function connect() {
        let opacityValue = 1;
        for (let a = 0; a < particles.length; a++) {
            for (let b = a; b < particles.length; b++) {
                let dx = particles[a].x - particles[b].x;
                let dy = particles[a].y - particles[b].y;
                let distance = dx * dx + dy * dy;

                if (distance < 15000) {
                    opacityValue = 1 - (distance / 15000);
                    ctx.strokeStyle = 'rgba(76, 29, 149,' + opacityValue * 0.6 + ')';
                    ctx.lineWidth = 1;
                    ctx.beginPath();
                    ctx.moveTo(particles[a].x, particles[a].y);
                    ctx.lineTo(particles[b].x, particles[b].y);
                    ctx.stroke();
                }
            }
        }
    }
    
    animate();

    return {
        destroy: () => {
            cancelAnimationFrame(animationFrameId);
            window.removeEventListener('resize', resize);
            window.removeEventListener('mousemove', null);
        }
    };
};

// Initialize on standard page load
document.addEventListener("DOMContentLoaded", () => {
    window.initInteractiveBg('interactive-bg');
});

// Re-initialize when navigating via Blazor Enhanced Navigation
if (typeof Blazor !== 'undefined') {
    Blazor.addEventListener('enhancedload', () => {
        window.initInteractiveBg('interactive-bg');
    });
}
