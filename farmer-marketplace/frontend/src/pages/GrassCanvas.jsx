import React, { useEffect, useRef } from 'react';

export default function GrassCanvas() {
  const canvasRef = useRef(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    let animationFrameId;

    // Height set to 110px
    const handleResize = () => {
      canvas.width = window.innerWidth;
      canvas.height = 110;
    };

    handleResize();
    window.addEventListener('resize', handleResize);

    // Generate detailed photorealistic grass blades
    const density = Math.floor(window.innerWidth / 1.2);
    const blades = [];

    for (let i = 0; i < density; i++) {
      const layer = Math.random() < 0.35 ? 1 : Math.random() < 0.7 ? 2 : 3;
      
      blades.push({
        x: Math.random() * window.innerWidth,
        // Shortened height profile (35px - 75px)
        height: layer === 1 ? 35 + Math.random() * 20 : layer === 2 ? 50 + Math.random() * 20 : 60 + Math.random() * 15,
        width: layer === 1 ? 3 + Math.random() * 2 : layer === 2 ? 4.5 + Math.random() * 2.5 : 6 + Math.random() * 3,
        layer,
        swayOffset: Math.random() * Math.PI * 2,
        // Significantly faster sway speed
        swaySpeed: 0.08 + Math.random() * 0.07,
        curve: (Math.random() - 0.5) * 12,
        // Realistic field green palette (lush yellow-greens to deep forestry greens)
        hue: 88 + Math.random() * 38,
        lightness: layer === 1 ? 18 : layer === 2 ? 28 : 38,
        // Adds 3D blade twist effect
        twist: (Math.random() - 0.5) * 6
      });
    }

    // Sort to render background blades first
    blades.sort((a, b) => a.layer - b.layer);

    let time = 0;

    const animate = () => {
      // Fast wind velocity time step
      time += 0.12; 
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // 1. Soft Ambient Ground Occlusion Shadow
      const groundShadow = ctx.createLinearGradient(0, canvas.height - 25, 0, canvas.height);
      groundShadow.addColorStop(0, 'rgba(8, 30, 12, 0)');
      groundShadow.addColorStop(1, 'rgba(4, 20, 8, 0.9)');
      ctx.fillStyle = groundShadow;
      ctx.fillRect(0, canvas.height - 25, canvas.width, 25);

      // 2. Render Fast-Swaying 3D Blades
      blades.forEach((b) => {
        // High-velocity wind calculation with secondary gusts
        const primaryWind = Math.sin(time * b.swaySpeed + b.swayOffset) * (22 + b.layer * 8);
        const gust = Math.cos(time * 0.2 + b.x * 0.005) * 10;
        const totalWind = primaryWind + gust;

        const tipX = b.x + b.curve + totalWind;
        const tipY = canvas.height - b.height;
        const cpX = b.x + (b.curve + totalWind) * 0.5 + b.twist;
        const cpY = canvas.height - b.height * 0.55;

        // Photorealistic lighting gradient per blade
        const bladeGrad = ctx.createLinearGradient(b.x, canvas.height, tipX, tipY);
        bladeGrad.addColorStop(0, `hsl(${b.hue - 10}, 80%, ${b.lightness - 14}%)`); // Root shadow
        bladeGrad.addColorStop(0.5, `hsl(${b.hue}, 85%, ${b.lightness}%)`);       // Mid body
        bladeGrad.addColorStop(0.88, `hsl(${b.hue + 8}, 90%, ${b.lightness + 18}%)`); // Sunlit body
        bladeGrad.addColorStop(1, `hsl(${b.hue + 15}, 95%, ${b.lightness + 28}%)`);  // Translucent tip highlight

        // Draw curved organic blade shape
        ctx.beginPath();
        ctx.moveTo(b.x - b.width / 2, canvas.height);
        ctx.quadraticCurveTo(cpX, cpY, tipX, tipY);
        ctx.quadraticCurveTo(cpX + b.width * 0.25, cpY, b.x + b.width / 2, canvas.height);
        ctx.closePath();

        ctx.fillStyle = bladeGrad;
        ctx.fill();

        // 3. Subtle specular highlight line along foreground blades for realistic shine
        if (b.layer === 3 && b.width > 5) {
          ctx.beginPath();
          ctx.moveTo(b.x, canvas.height - 10);
          ctx.quadraticCurveTo(cpX, cpY, tipX - 1, tipY + 2);
          ctx.strokeStyle = `hsla(${b.hue + 20}, 100%, 85%, 0.35)`;
          ctx.lineWidth = 0.8;
          ctx.stroke();
        }
      });

      animationFrameId = requestAnimationFrame(animate);
    };

    animate();

    return () => {
      window.removeEventListener('resize', handleResize);
      cancelAnimationFrame(animationFrameId);
    };
  }, []);

  return (
    <canvas
      ref={canvasRef}
      style={{
        position: 'fixed',
        bottom: 0,
        left: 0,
        width: '100vw',
        height: '110px',
        pointerEvents: 'none',
        zIndex: 50
      }}
    />
  );
}