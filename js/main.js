document.addEventListener('DOMContentLoaded', function() {
    // Smooth scrolling for navigation links
    const navLinks = document.querySelectorAll('nav a');
    
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            
            const targetId = this.getAttribute('href');
            const targetSection = document.querySelector(targetId);
            
            window.scrollTo({
                top: targetSection.offsetTop,
                behavior: 'smooth'
            });
        });
    });
    
    // Add animation to elements when they come into view
    const animateOnScroll = function() {
        const sections = document.querySelectorAll('section');
        
        sections.forEach(section => {
            const sectionTop = section.getBoundingClientRect().top;
            const windowHeight = window.innerHeight;
            
            if (sectionTop < windowHeight * 0.75) {
                section.classList.add('visible');
            }
        });
    };
    
    // Initial check for elements in view
    animateOnScroll();
    
    // Check for elements in view on scroll
    window.addEventListener('scroll', animateOnScroll);
    
    // Mobile navigation toggle
    const createMobileNav = function() {
        const nav = document.querySelector('nav');
        const navList = nav.querySelector('ul');
        
        // Create mobile nav toggle button
        const mobileToggle = document.createElement('button');
        mobileToggle.classList.add('mobile-nav-toggle');
        mobileToggle.innerHTML = '<span></span><span></span><span></span>';
        nav.insertBefore(mobileToggle, navList);
        
        // Toggle mobile navigation
        mobileToggle.addEventListener('click', function() {
            navList.classList.toggle('active');
            this.classList.toggle('active');
        });
    };
    
    // Only create mobile nav for smaller screens
    if (window.innerWidth < 768) {
        createMobileNav();
    }
    
    // Update mobile nav on window resize
    window.addEventListener('resize', function() {
        if (window.innerWidth < 768) {
            if (!document.querySelector('.mobile-nav-toggle')) {
                createMobileNav();
            }
        } else {
            const mobileToggle = document.querySelector('.mobile-nav-toggle');
            if (mobileToggle) {
                mobileToggle.remove();
                document.querySelector('nav ul').classList.remove('active');
            }
        }
    });
});