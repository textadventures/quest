/* Based on jjmenu 1.1.2 by Jacek Jursza (okhan.pl@gmail.com)
 * http://jursza.net/dev/jjmenu/
 *  
 * copyright (c) 2009 Jacek Jursza (http://jursza.net/)
 * licence MIT [http://www.opensource.org/licenses/mit-license.php]    
 */

(function ($) {

    $.fn.jjmenu = function (param) {

        this.click(function (event) {
            event.preventDefault();
            event.stopPropagation();
            menu(this);
            $(this).blur();
            return false;
        });

        $(document).click(function (event) { if (event.button != 2) $("div[id^=jjmenu]").remove(); });

        function menu(el) {
            $("div[id^=jjmenu_main]").remove();

            var m = document.createElement('div');
            var ms = document.createElement('span');
            $(m).append(ms);

            m.className = "jjmenu";
            m.id = "jjmenu_main";
            $(m).css({ display: 'none' });
            $(document.body).append(m);

            positionMenu();

            for (var i in param) {
                putItem(param[i]);
            }

            checkPosition();
            showMenu();

            function positionMenu() {
                var pos = $(el).offset();
                var t = pos.top;
                var l = pos.left;
                $(m).css({ position: "absolute", top: t + "px", left: l + "px" });
            }

            function checkPosition() {

                var isHidden = $(m).css("display") == "none";
                if (isHidden) $(m).show();
                
                var positionTop = $(m).offset().top;
                var positionLeft = $(m).offset().left;
                if (isHidden) $(m).hide();

                var xPos = positionTop - $(window).scrollTop();

                $(m).css({ left: "0px" });
                var menuHeight = $(m).outerHeight();
                var menuWidth = $(m).outerWidth();
                $(m).css({ left: positionLeft + "px" });

                var nleft = positionLeft;
                if (positionLeft + menuWidth > $(window).width()) {
                    nleft = $(window).width() - menuWidth;
                }

                var spaceBottom = true;

                if (xPos + menuHeight + $(el).outerHeight() > $(window).height()) {
                    spaceBottom = false;
                }

                var spaceTop = true;
                
                if (positionTop - menuHeight < 0) {
                    spaceTop = false;
                }

                var ntop;

                if (!spaceBottom && spaceTop) {
                    // top orientation
                    ntop = parseInt(positionTop, 10) - parseInt(menuHeight, 10);
                    $(m).addClass("topOriented");

                } else {
                    // bottom orientation
                    $(m).addClass("bottomOriented");
                    positionTop = positionTop + $(el).outerHeight();
                    ntop = parseInt(positionTop, 10);
                }

                $(m).css({ "top": ntop + "px", "left": nleft + "px" });
            }

            function showMenu() {
                var speed = 100;
                $(m).fadeIn(speed);
            }

            function putItem(n) {
                var item = document.createElement('div');
                $(item).hover(function () {
                    $(this).addClass("jj_menu_item_hover");
                },
                function () {
                    $(this).removeClass("jj_menu_item_hover");
                });

                $(item).click(function (event) {
                    event.stopPropagation();
                    $("div[id^=jjmenu]").remove();
                    n.action.callback(n.title);
                });

                var span = document.createElement('span');
                $(item).append(span);

                item.className = "jj_menu_item";

                $(span).html(n.title);
                $(ms).append(item);
            }
        }
    };

})(jQuery);