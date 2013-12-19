jQuery('html').removeClass('no-js').addClass('js');

$(window).load(function() {

    /* ==============================================
    Preloader
    =============================================== */
    var preloaderDelay = 350,
        preloaderFadeOutTime = 800;

    function hidePreloader() {
        var loadingAnimation = $('#loader-animation'),
            preloader = $('#preloader');

        loadingAnimation.fadeOut();
        preloader.delay(preloaderDelay).fadeOut(preloaderFadeOutTime);
    }

    hidePreloader();

});

$(document).ready(function() {

    var wWidth = $(window).width(),
        mobileRes = 767;

    /* ==============================================
    Section Position
    =============================================== */
    function setSections() {
        var sections = $("section"),
            wWidth = $(window).width(),
            wCounter = 0;
        
        if(wWidth > mobileRes) {  

            $.each(sections, function(eq) {
                if(eq > 0) {
                    $(this).css({'left': wCounter});
                }
                wCounter = wCounter + $(this).width();            
            }); 

        } else {
            $.each(sections, function(eq) {
                $(this).css({'left': 0});          
            }); 
        }     
    }

    function forcePosition() {
        var hash = window.location.hash,
        $panels = $('section');
        $panels.each(function(eq) {
            var panelId = $(this).attr('id');
            if( '#' + panelId == hash ) {
                var wWidth = $(window).width(),
                    scrollElement = 'html, body';

                $(scrollElement).stop().animate({
                    scrollLeft: wWidth * eq
                }, 300, 'easeOutCubic');
    
            }
        });
    }

    function resetWindowWidth() {
        wWidth = $(window).width();
    }

    $(window).resize(function() {
        setSections();
        forcePosition();
        resetWindowWidth();
    });

    setSections();

    /* ==============================================
    Navigation
    =============================================== */
    var noIntro = $('body').hasClass('no-intro');

    function setNavigation() {
        var nav = $('nav#main-nav'),
            wWidth = $(window).width();
            
        if( !noIntro ) {
            if(wWidth > mobileRes) {
                nav.css({'left': wWidth });
            }
        }
        
    }

    setNavigation();

    function adjustNavigation() {
        var nav = $('nav#main-nav'),
            scroll = $(window).scrollLeft(),
            wWidth = $(window).width();

        if( !noIntro ) {
            if(wWidth > mobileRes) {
               if(scroll >= wWidth) {
                    nav.css({
                        'left': 0
                    });
                } else {
                    nav.css({
                        'left': wWidth - scroll
                    });
                } 
            } else {
                nav.css({
                    'left': 0
                });
            }     
        }
        
    }

    $(window).scroll(function() {
        adjustNavigation();
    });

    $(window).resize(function() {
        adjustNavigation();
    });

    /* ==============================================
    Mobile Navigation
    =============================================== */

    $(function() {  
        var trigger = $('#responsive-nav');  
            menu = $('#main-nav ul');
      
        $(trigger).on('click', function(e) {  
            e.preventDefault();  
            menu.slideToggle();
            $(this).toggleClass('nav-visible');
        }); 

        $(window).resize(function(){  
            var windowW = $(window).width();  
            if(windowW > mobileRes && menu.is(':hidden')) {  
                menu.removeAttr('style');
            }  
        }); 
    });  

    /* ==============================================
    Smooth Scrolling
    =============================================== */
    var scrollElement = 'html, body',
        $scrollElement;

    $(function() {

        $('html, body').each(function () {
            if(wWidth > mobileRes) {
                var initScrollLeft = $(this).attr('scrollLeft');
            
                $(this).attr('scrollLeft', initScrollLeft + 1);
                if ($(this).attr('scrollLeft') == initScrollLeft + 1) {
                    scrollElement = this.nodeName.toLowerCase();
                    $(this).attr('scrollLeft', initScrollLeft);
                    return false;
                }
            }
                
        });
        $scrollElement = $(scrollElement);
    });

    $(function() {
        var $sections = $('section.section');  

        $sections.each(function() {
            var $section = $(this);
            var hash = '#' + this.id;

            $('a[href="' + hash + '"]').click(function(event) {
                moving = true;
                if(wWidth > mobileRes) {
                    $scrollElement.stop().animate({
                        scrollLeft: $section.offset().left
                    }, 1200, 'easeOutCubic', function() {
                        window.location.hash = hash;
                    });
                } else {
                    $scrollElement.stop().animate({
                        scrollTop: $section.offset().top
                    }, 1200, 'easeOutCubic');
                }
                $('nav#main-nav a').removeClass('active');
                if($(this).hasClass('content-menu-link')) {
                    var link = $(this).attr('href');
                    $('a[href="' + hash + '"]').addClass('active');
                } else {
                    $(this).addClass('active');
                }

                var trigger = $('#responsive-nav'),
                    menu = $('#main-nav ul'); 

                if(trigger.hasClass('nav-visible')) {
                    menu.slideToggle();
                    trigger.toggleClass('nav-visible');
                }

                event.preventDefault();
            });
        });

    });

    function setInitActiveMenu() {
        var hash = window.location.hash;
        $('a[href="' + hash + '"]').addClass('active');
    }

    setInitActiveMenu();

    /* ==============================================
    Mobile Menu
    =============================================== */
    if ($('.mobile-nav').length && $('.mobile-nav').attr('data-autogenerate') == 'true') {
        var mobileMenu = $('.mobile-nav');
        $('ul.nav li a').each(function(index, elem) {
            mobileMenu.append($('<option></option>').val($(elem).attr('href')).html($(elem).html()));
        });
    }

    $('.mobile-nav').on('change', function() {
        link = $(this).val();
        if (!link) {
            return;
        }

        if (link.substring(0,1) == '#') {
            $('html, body').animate({scrollTop: $(link).offset().top - 74}, 750);
        } else {
            document.location.href = link;
        }
    });

    /* ==============================================
    Fancybox
    =============================================== */
    function bindFancybox() {
        $("a.fancybox").fancybox({
            'overlayShow'   : false,
            'transitionIn'  : 'elastic',
            'transitionOut' : 'elastic'
        });
    }

    bindFancybox();

    /* ==============================================
    Input Placeholder for IE
    =============================================== */

    if(!Modernizr.input.placeholder){

        $('[placeholder]').focus(function() {
            var input = $(this);
            if (input.val() == input.attr('placeholder')) {
                input.val('');
                input.removeClass('placeholder');
            }
        }).blur(function() {
            var input = $(this);
            if (input.val() == '' || input.val() == input.attr('placeholder')) {
                input.addClass('placeholder');
                input.val(input.attr('placeholder'));
            }
        }).blur();
        $('[placeholder]').parents('form').submit(function() {
            $(this).find('[placeholder]').each(function() {
                var input = $(this);
                if (input.val() == input.attr('placeholder')) {
                    input.val('');
                }
            });
        });

    }

    /* ==============================================
    Portfolio
    =============================================== */
    $(window).load(function(){

        var $container = $('.portfolio');
        $container.isotope({
            filter: '*',
            animationOptions: {
                duration: 750,
                easing: 'linear',
                queue: false
            }
        });

        $('nav.portfolio-filter ul a').click(function(){
            var selector = $(this).attr('data-filter');
            $container.isotope({
                filter: selector,
                animationOptions: {
                    duration: 750,
                    easing: 'linear',
                    queue: false
                }
            });
          return false;
        });

        var $optionSets = $('nav.portfolio-filter ul'),
               $optionLinks = $optionSets.find('a');
         
               $optionLinks.click(function(){
                  var $this = $(this);
              // don't proceed if already selected
              if ( $this.hasClass('selected') ) {
                  return false;
              }
           var $optionSet = $this.parents('nav.portfolio-filter ul');
           $optionSet.find('.selected').removeClass('selected');
           $this.addClass('selected'); 
        });

        $(window).resize(function () {
            $container.isotope('reLayout');
        });

        var $portfolioContainer = $('#portfolio > .container'),
            $portfolioSingle = $('.single-portfolio'),
            portfolioSingleH = 0;
            portfolioActive = false;

        function loadPortfolio(fileToLoad) {
            $portfolioSingle.load(fileToLoad, function() {
                portfolioSingleH = $portfolioSingle.find('.container').outerHeight();

                checkVideo(false);

                $portfolioSingle.css({
                    'top': -portfolioSingleH
                });
                if(wWidth > mobileRes) {
                    $('#portfolio').animate({ scrollTop: 0 }, "slow");
                    $portfolioSingle.stop().animate({
                        'top': 0
                    }, 500, 'easeOutCubic');
                    $portfolioContainer.stop().animate({
                        'marginTop': portfolioSingleH
                    });
                } else {
                    var portfolioPos = $('#portfolio').offset().top;
                    $scrollElement.animate({ scrollTop: portfolioPos}, "slow");
                    $portfolioSingle.stop().animate({
                        'top': 0
                    }, 500, 'easeOutCubic');
                    $portfolioContainer.stop().animate({
                        'marginTop': portfolioSingleH
                    });
                }
                         
                portfolioActive = true;
                bindClosePortfolio();
                bindFancybox();
                setupFlexslider();
            });
        }

        function checkVideo(removeSource) {
            var isVideo = ($portfolioSingle.find('.video-container').length > 0) ? true : false;
            if(isVideo) {
                var videoIframe = $portfolioSingle.find('iframe'),
                    videoSource = videoIframe.attr('src');
                if(removeSource) {
                    videoIframe.attr('src', '');
                } else {
                    if(videoSource == '') {
                        var setupSource = videoIframe.data('source');
                        videoIframe.attr('src', setupSource);
                    } else {
                        videoIframe.data('source', videoSource);
                    }
                }     
            }
        }

        function changePortfolio(fileToLoad) {
            $portfolioSingle.load(fileToLoad, function() {
                portfolioActive = true;
                portfolioSingleH = $portfolioSingle.find('.container').outerHeight();
                $('#portfolio').animate({ scrollTop: 0 }, "slow");
                $portfolioSingle.animate({
                    'height': portfolioSingleH
                }, 500);
                $portfolioContainer.stop().animate({
                    'marginTop': portfolioSingleH
                }, 500);
                bindClosePortfolio();
                bindFancybox();
                setupFlexslider();
            });
        }

        function bindClosePortfolio() {
            $portfolioSingle.find('.portfolio-close').bind('click', function() {

                checkVideo(true);
                closePortfolio(false);
                    
            });
        }

        function closePortfolio(reopen, fileToLoad) {
            if(reopen) {
                   changePortfolio(fileToLoad) 
            } else {
                $portfolioSingle.stop().animate({
                    'top': -portfolioSingleH
                }, 500, 'easeOutCubic', function() {
                    portfolioActive = false;
                    $portfolioSingle.css({
                        'height': 'auto'
                    });
                });
                $portfolioContainer.stop().animate({
                    'marginTop': 0
                });
            }
            
        }

        $(".portfolio .portfolio-item a").on('click', function(event) {

            event.preventDefault();

            var fileToLoad = $(this).data('file');

            if(portfolioActive) {
                closePortfolio(true, fileToLoad);
            } else {
                loadPortfolio(fileToLoad);
            }
            
        });

        $(window).resize(function () {
            portfolioSingleH = $portfolioSingle.find('.container').outerHeight();
            var $closeButton = $portfolioSingle.find('.portfolio-close');

            $portfolioSingle.css({
                'height': portfolioSingleH
            });

            $portfolioContainer.css({
                'marginTop': portfolioSingleH
            });

            $closeButton.unbind('click');
            $closeButton.bind('click', function() {
                closePortfolio(false);
            });

            if(!portfolioActive) {
                $portfolioSingle.css({
                    'top': -portfolioSingleH
                });
                $portfolioContainer.css({
                    'marginTop': 0
                });
            }

        });

        /* ==============================================
        Flexslider
        =============================================== */

        function setupFlexslider() {

            $('.flexslider').flexslider({
                pauseOnHover: true,
                directionNav: true,
                controlNav: false
            });

        }

        setupFlexslider();

        /* ==============================================
        Google Maps
        =============================================== 

        var $mapTrigger = $('#show-map'),
            $map = $('#map'),
            $contactForm = $('.contact-form'),
            mapOptions = {
                center: new google.maps.LatLng(40.717762,-73.970518),
                zoom: 15,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            },
            map,
            mapActiveClass = 'map-active',
            mapFadeTime = 500,
            mapShowText = 'Show Map',
            mapHideText = 'Hide Map',
            mapMarkerTitle = 'Reversal',
            $mainNavLinks = $('nav#main-nav a'),
            contactID = "contacts",
            $contactsContainer = $('#contacts .container');

        $mapTrigger.on('click', function(event) {

            event.preventDefault();
            var $this = $(this);
            if($this.hasClass(mapActiveClass)) {
                hideMap();      
            } else {
                showMap();               
            }
            
        });

        function hideMap() {
            $contactForm.fadeIn(mapFadeTime);
            $mapTrigger.removeClass(mapActiveClass).html(mapShowText);
            $map.fadeOut(mapFadeTime, function() {
                $(this).empty();
            });
        }

        function showMap() {
            $contactForm.fadeOut(mapFadeTime, function() {
                var cHeight = $contactsContainer.outerHeight(),
                    wHight = $(window).height();
                if(cHeight > wHight) {
                    $map.css({ 'height' : cHeight }).show(); 
                } else {
                    $map.show();
                }
                map = new google.maps.Map(document.getElementById("map"),mapOptions);
                var marker = new google.maps.Marker({
                        position: mapOptions.center,
                        title: mapMarkerTitle
                    });
                marker.setMap(map);
                $mapTrigger.addClass(mapActiveClass).html(mapHideText);
            }); 
        }

        $mainNavLinks.on('click', function() {
            if($(this).attr('href') != "#" + contactID) {
                hideMap();
            }
        });
    */

    });

    /* ==============================================
    Form validation
    =============================================== */
    var $contactForm = $('.contact-form'),
        formErrorClass = 'form-error',
        $responseMessage = $('.response-message');

    $.validator.setDefaults({
        submitHandler: function() {

            $.ajax({
                type: $contactForm.attr('method'),
                url: $contactForm.attr('action'),
                data: $contactForm.serialize(),
                success: function(data) {
                    $responseMessage.html(data);
                }
             });

        },
        highlight: function(input) {
                $(input).addClass(formErrorClass);
        },
        unhighlight: function(input) {
                $(input).removeClass(formErrorClass);
        }
    });

    jQuery($contactForm).validate({
        messages: {
            name: {
                required: ''
            },
            email: '',
            message: '',
            check: ''
        }
    });

});