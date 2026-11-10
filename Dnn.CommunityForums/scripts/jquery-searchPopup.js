(function ($) {
    $.fn.afSearchPopup = function () {

        return this.each(function () {

            var obj = $(this);

            obj.data('visible', false);

            var popup = obj.find('.aftb-search-popup');

            popup.css('left', (obj.width() - popup.width()) + "px");
            popup.css('top', obj.height() + "px");

            $('span', obj).first().click(function () {

                obj.data('visible', !obj.data('visible'));

                $('.fa-caret-down, .fa-caret-up', this).toggleClass('fa-caret-down fa-caret-up');
                $('.aftb-search-popup', obj).toggle();

            });

            $(':text', obj).keypress(function (event) {
                if (event.keyCode == 13) {
                    $('button', obj).click();
                }
            });

            $('button', obj).click(function () {

                var query = obj.find('[type=text]').val();
                query = $.trim(query);
                if (!query)
                    return false;

                var searchUrl = obj.attr('data-searchUrl');

                document.location.href = searchUrl + "?q=" + query;
                
                return false;
            });

            $(document).mouseup(function (e) {

                if (obj.has(e.target).length === 0 && obj.data('visible')) {
                    obj.data('visible', false);
                    $('.fa-caret-down, .fa-caret-up', obj).toggleClass('fa-caret-down fa-caret-up');
                    $('.aftb-search-popup', obj).toggle();
                }
                    
            });

        });
    };
})(jQuery);
