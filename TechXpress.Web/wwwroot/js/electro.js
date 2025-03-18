(function($) {
	"use strict"

	// Mobile Nav toggle
	$('.menu-toggle > a').on('click', function (e) {
		e.preventDefault();
		$('#responsive-nav').toggleClass('active');
	})

	// Fix cart dropdown from closing
	$('.cart-dropdown').on('click', function (e) {
		e.stopPropagation();
	});

	$('.dropdown-toggle').on('click', function (e) {
		e.preventDefault();
		$(this).next('.dropdown-menu').toggleClass('show');
	});

	document.addEventListener("DOMContentLoaded", function () {
		var cartDropdownButton = document.getElementById("cartDropdown");
		var cartDropdownMenu = document.querySelector(".cart-dropdown");

		cartDropdownButton.addEventListener("click", function (event) {
			event.stopPropagation(); // Prevent closing when clicking the button
			cartDropdownMenu.classList.toggle("show");
		});

		// Close dropdown when clicking anywhere outside
		document.addEventListener("click", function (event) {
			if (!cartDropdownButton.contains(event.target) && !cartDropdownMenu.contains(event.target)) {
				cartDropdownMenu.classList.remove("show");
			}
		});
	});


	/////////////////////////////////////////

	document.addEventListener("DOMContentLoaded", function () {
		var priceInputMax = document.getElementById('price-max');
		var priceInputMin = document.getElementById('price-min');

		if (priceInputMax) {
			priceInputMax.addEventListener('change', function () {
				updatePriceSlider($(this).parent(), this.value);
			});
		}

		if (priceInputMin) {
			priceInputMin.addEventListener('change', function () {
				updatePriceSlider($(this).parent(), this.value);
			});
		}
	});

	// Products Slick
	$('.products-slick').each(function() {
		var $this = $(this),
				$nav = $this.attr('data-nav');

		$this.slick({
			slidesToShow: 4,
			slidesToScroll: 1,
			autoplay: true,
			infinite: true,
			speed: 300,
			dots: false,
			arrows: true,
			appendArrows: $nav ? $nav : false,
			responsive: [{
	        breakpoint: 991,
	        settings: {
	          slidesToShow: 2,
	          slidesToScroll: 1,
	        }
	      },
	      {
	        breakpoint: 480,
	        settings: {
	          slidesToShow: 1,
	          slidesToScroll: 1,
	        }
	      },
	    ]
		});
	});

	// Products Widget Slick
	$('.products-widget-slick').each(function() {
		var $this = $(this),
				$nav = $this.attr('data-nav');

		$this.slick({
			infinite: true,
			autoplay: true,
			speed: 300,
			dots: false,
			arrows: true,
			appendArrows: $nav ? $nav : false,
		});
	});

	/////////////////////////////////////////

	// Product Main img Slick
	$('#product-main-img').slick({
    infinite: true,
    speed: 300,
    dots: false,
    arrows: true,
    fade: true,
    asNavFor: '#product-imgs',
  });

	// Product imgs Slick
  $('#product-imgs').slick({
    slidesToShow: 3,
    slidesToScroll: 1,
    arrows: true,
    centerMode: true,
    focusOnSelect: true,
		centerPadding: 0,
		vertical: true,
    asNavFor: '#product-main-img',
		responsive: [{
        breakpoint: 991,
        settings: {
					vertical: false,
					arrows: false,
					dots: true,
        }
      },
    ]
  });

	// Product img zoom
	var zoomMainProduct = document.getElementById('product-main-img');
	if (zoomMainProduct) {
		$('#product-main-img .product-preview').zoom();
	}

	/////////////////////////////////////////

	// Input number
	// Input number

	//$('.input-number').each(function () {
	//	var $this = $(this),
	//		$input = $this.find('input[type="number"]'),
	//		up = $this.find('.qty-up'),
	//		down = $this.find('.qty-down');

	//	down.on('click', function () {
	//		var value = parseInt($input.val()) - 1;
	//		value = value < 1 ? 1 : value;
	//		$input.val(value);
	//		// Only trigger updatePriceSlider if this is a price-related input
	//		if ($this.hasClass('price-min') || $this.hasClass('price-max')) {
	//			updatePriceSlider($this, value);
	//		}
	//		// Trigger change event separately
	//		$input.trigger('change');
	//	});

	//	up.on('click', function () {
	//		var value = parseInt($input.val()) + 1;
	//		$input.val(value);
	//		// Only trigger updatePriceSlider if this is a price-related input
	//		if ($this.hasClass('price-min') || $this.hasClass('price-max')) {
	//			updatePriceSlider($this, value);
	//		}
	//		// Trigger change event separately
	//		$input.trigger('change');
	//	});
	//});

	//// Safer implementation of updatePriceSlider

	//var priceInputMax = document.getElementById('price-max'),
	//		priceInputMin = document.getElementById('price-min');

	//priceInputMax.addEventListener('change', function(){
	//	updatePriceSlider($(this).parent() , this.value)
	//});

	//priceInputMin.addEventListener('change', function(){
	//	updatePriceSlider($(this).parent() , this.value)
	//});

	//function updatePriceSlider(elem, value) {
	//	// Check if priceSlider exists and has noUiSlider initialized
	//	if (!priceSlider || !priceSlider.noUiSlider) {
	//		return;
	//	}

	//	if (elem.hasClass('price-min')) {
	//		console.log('min', value);
	//		priceSlider.noUiSlider.set([value, null]);
	//	} else if (elem.hasClass('price-max')) {
	//		console.log('max', value);
	//		priceSlider.noUiSlider.set([null, value]);
	//	}
	//}

	//// Price Slider
	//// Price Slider
	//var priceSlider = document.getElementById('price-slider');
	//if (priceSlider) {
	//	try {
	//		noUiSlider.create(priceSlider, {
	//			start: [1, 999],
	//			connect: true,
	//			step: 1,
	//			range: {
	//				'min': 1,
	//				'max': 999
	//			}
	//		});
	//		priceSlider.noUiSlider.on('update', function (values, handle) {
	//			var value = values[handle];
	//			handle ? priceInputMax.value = value : priceInputMin.value = value;
	//		});
	//	} catch (e) {
	//		console.error("Error initializing price slider:", e);
	//	}
	//}

	$('#footer').css('display', 'block');
	$('#bottom-footer').css('display', 'block');

})(jQuery);
