// -----------------------------
// index.html 页面模块功能 -- mobile
// 
// document.ready()
//   --监听：手指向上滑动
//   --
// 
// 移动端：目录导航
// 
// 
// 移动端主目录: 展开折叠，目录功能
// 
// 
console.log("●● wt_index_m.js");


// -----------------------------
// document.ready()
//

$(document).ready(function () {
	console.log("●●●● index mobile > ready()");
	
    var isTouchEnabled = false;
    try {
        if (document.createEvent("TouchEvent")) {
           isTouchEnabled = true;
        }
    } catch (e) {
        //debug(e);
    }
	// 接收 iframe 内消息
	addEventListener('message', e => {
		// 都在 wt_index.js 中处理
	});
	
	
});

// -----------------------------
// document.addEventListener()
// --- 监听窗口的滚动事件
//
//
var startX, startY;  
document.addEventListener('touchstart', function(ev){
    startX = ev.touches[0].pageX;
    startY = ev.touches[0].pageY;
}, false);
document.addEventListener('touchend',function (ev) {  
    var endX, endY;  
    endX = ev.changedTouches[0].pageX;  
    endY = ev.changedTouches[0].pageY;  
    var direction = getSlideDirection(startX, startY, endX, endY);
    switch(direction){
        case 0:
            break;
        case 1:
            // 手指向上滑动
            var scrollTop = 0;
            var scrollTimer = setInterval(function(){
				if(scrollTop != $(document).scrollTop()){
					scrollTop = $(document).scrollTop();
				}else{
					scrollTop = $(document).scrollTop();
					clearInterval(scrollTimer);
					// 以下样式没有被使用 窗口滑动隐藏
					if($(document).scrollTop() > 60){
		            	$('.wt_header').css('display', 'none');
		            	$('.wt_main_page_search').css('display', 'none');
		            	$('.wt_search_input').css('display', 'none');
		            }
				}
			}, 50);
            break;
        case 2:
            // 手指向下滑动
            var scrollTop = 0;
            var scrollTimer = setInterval(function(){
				if(scrollTop != $(document).scrollTop()){
					scrollTop = $(document).scrollTop();
				}else{
					scrollTop = $(document).scrollTop();
					clearInterval(scrollTimer);
					// 以下样式没有被使用
					if($(document).scrollTop() <= 60){
		            	$('.wt_header').css('display', 'block');
		            	$('.wt_main_page_search').css('display', 'block');
		            	$('.wt_search_input').css('display', 'block');
		            }
				}
			}, 50);
            break;
		case 3:
			//向右滑动
			break;
		case 4:
			//向左滑动
			break;
    }
}, false);
function getSlideDirection(startX, startY, endX, endY) {  
    var dy = startY - endY;  
    var dx = endX - startX;  
    var result = 0; 
    if(dy > 0){//向上滑动
        return 1;
    }else{//向下滑动
        return 2;
    }
    if(dx > 0){//向右滑动
    	return 3;
    }else{//向左滑动
    	return 4;
    } 
}




// -----------------------------
// 移动端主目录: 显示，关闭
//
function openMobileMenu(){
	// wt_main_page 不隐藏，放在底层
	//$(".wt_main_menu").show();
	$(".wt_menu_page").addClass('menu_show');
	// 隐藏 footer
	$(".wt_footer").hide();
	$(".wt_footer_mobile").hide();
	// mobile_menu_custom_tip  定制状态消息提示
	menuCustomTipDisplayMobile();
	
	// 滚动到目录激活章节位置
	if($('#div_menu_header_ul .active').length != 0){
		var liOffsetTop = $('#div_menu_header_ul .active').offset().top;
		var menuHeight = $('#div_menu_header_ul').eq(0).height();
		var menuScrollTop = $('#div_menu_header_ul').eq(0).scrollTop();
		// 目录搜索框下边框 位置 menuOffsetTop = 106
		var menuOffsetTop = $('#div_menu_header_ul').eq(0).offset().top;
		if (menuHeight < liOffsetTop) {
			$('#div_menu_header_ul').eq(0).scrollTop(menuScrollTop + liOffsetTop - 106);
		} else if(liOffsetTop < menuOffsetTop){
			$('#div_menu_header_ul').eq(0).scrollTop(menuScrollTop + liOffsetTop - 106);
		}
	}
	
}
function closeMobileMenu(){
	$(".wt_menu_page").removeClass('menu_show');
	$(".wt_footer").show();
	$(".wt_footer_mobile").show();
}

// 打开本页导航
function openTopicAnchorList() {
	if ($('#frame_element').contents().find('.right_guide').find('li').length > 0) {
		console.log("打开本页导航");
		// 清空 目录导航
		$('.wt_footer_mobile_anchor_list').html("");
		$('.wt_footer_mobile_anchor_container').removeClass('hidden');
		// header(wt_footer_mobile_anchor_header) : feature_guide
		var headerText = $('#frame_element').contents().find('.feature_guide').children('div').text();
		$('.wt_footer_mobile_anchor_header').children('span').eq(0).text(headerText);
		// list 
		// 修改后的遍历逻辑
		$.each($('#frame_element').contents().find('.right_guide').find('li'), function(i, ele) {
		var $li = $(ele);
		var anchor_item = $('<div></div>');
		anchor_item.addClass('wt_anchor_item');

		// 根据桌面端元素的层级类添加对应的移动端类
		if ($li.hasClass('feature_guide_h2')) {
			anchor_item.addClass('wt_anchor_h2');
		} else if ($li.hasClass('feature_guide_h3')) {
			anchor_item.addClass('wt_anchor_h3');
		} else {
			anchor_item.addClass('wt_anchor_h1'); // 默认h1
		}

		// 处理active状态
		if ($li.hasClass('active')) {
			anchor_item.addClass('active');
		}

		// 复制属性和内容
		anchor_item.attr('data-id', $li.attr('data-id'))
			.attr('onclick', 'locationTopicAnchor($(this))')
			.html($li.html());
		
		$('.wt_footer_mobile_anchor_list').append(anchor_item);
		});
		// 滚动到active
		var activeItemTop = $('.wt_footer_mobile_anchor_list .active').position().top;
		var listHeight = $('.wt_footer_mobile_anchor_list').height();
		if(activeItemTop > listHeight) {
			$('.wt_footer_mobile_anchor_list').scrollTop(activeItemTop);
		} else if(activeItemTop < 0){
			$('.wt_footer_mobile_anchor_list').scrollTop(0);
		}
		
	} else {
		// 显示提示消息
		toastr.options = {
            closeButton: false,  
            debug: false,  
            progressBar: false,  
            positionClass: "toast-center-center",  
            onclick: null,  
            showDuration: "300",  
            hideDuration: "1000",  
            timeOut: "2000",  
            extendedTimeOut: "1000",  
            showEasing: "swing",  
            hideEasing: "linear",  
            showMethod: "fadeIn",  
            hideMethod: "fadeOut"  
        };
		var noAnchorText;
		if(getLang()){
			noAnchorText = getLocalization("webhelp.anchor.noanchor");
		} else {
			noAnchorText = getLocalization("webhelp.anchor.noanchor.en");
		}
		toastr.info(noAnchorText);
		
	}
}
// 关闭目录导航
function closeLocationTopic() {
	$('.wt_footer_mobile_anchor_container').addClass('hidden');
}

// 定位跳转 20250820修改
function locationTopicAnchor(obj) {
    obj.siblings().removeClass('active');
    obj.addClass('active');
    $('.wt_footer_mobile_anchor_container').addClass('hidden');
    
    var targetId = obj.attr('data-id');
    var frameWindow = document.getElementById("frame_element").contentWindow;
    var frameDoc = frameWindow.document;
    var $frame = $(frameDoc);
    
    // 精确目标元素定位
    var targetElement = $frame.find('#' + targetId);
    console.log('DEBUG 目标元素ID:', targetId, '元素:', targetElement.length ? targetElement.prop('tagName') : '未找到');

    if (targetElement.length) {
        // 更精确的类型判断
        let actualTargetElement = targetElement;
        let elementType = '';
        
        // 如果是div容器，尝试找到具体的标题元素
        if (targetElement.is('div')) {
            // 按优先级查找：H2 > H3 > H4
            const h2Title = targetElement.find('h2.title:has(.wh_expand_btn)').first();
            const h3Title = targetElement.find('h3.sectiontitle:has(.wh_expand_btn), h3.title:has(.wh_expand_btn)').first();
            const h4Title = targetElement.find('h4.title.sectiontitle:has(.wh_expand_btn), h4.title:has(.wh_expand_btn)').first();
            
            if (h2Title.length) {
                actualTargetElement = h2Title;
                elementType = 'H2';
            } else if (h3Title.length) {
                actualTargetElement = h3Title;
                elementType = 'H3';
            } else if (h4Title.length) {
                actualTargetElement = h4Title;
                elementType = 'H4';
            }
        } else {
            // 直接是标题元素
            if (targetElement.is('h2.title:has(.wh_expand_btn)')) {
                elementType = 'H2';
            } else if (targetElement.is('h3.sectiontitle:has(.wh_expand_btn), h3.title:has(.wh_expand_btn)')) {
                elementType = 'H3';
            } else if (targetElement.is('h4.title.sectiontitle:has(.wh_expand_btn), h4.title:has(.wh_expand_btn)')) {
                elementType = 'H4';
            }
        }
        
        console.log('DEBUG 元素类型:', elementType, '实际目标元素:', actualTargetElement.length ? actualTargetElement.prop('tagName') + '.' + actualTargetElement.attr('class') : '未找到');

        // H2处理逻辑
        if (elementType === 'H2') {
            const h2ExpandBtn = actualTargetElement.find('.wh_expand_btn');
            console.log('DEBUG H2按钮:', h2ExpandBtn.length, '是否展开:', h2ExpandBtn.hasClass('expanded'));
            
            // 如果未展开，则触发展开
            if (h2ExpandBtn.length && !h2ExpandBtn.hasClass('expanded')) {
                // 先触发点击，再处理滚动
                h2ExpandBtn[0].click();
                console.log('DEBUG H2程序化展开');
            }
            
            // 滚动到目标位置 - 确保标题在顶部
            setTimeout(() => {
                const targetOffset = actualTargetElement.offset();
                if (targetOffset) {
                    const targetTop = targetOffset.top;
                    // 直接使用targetTop，因为它已经是从iframe顶部开始计算的偏移量
                    const finalPosition = Math.max(0, targetTop - 10); // 减去10像素确保完全可见，但不低于0
                    console.log('DEBUG H2滚动到位置:', finalPosition);
                    frameWindow.scrollTo({
                        top: finalPosition,
                        behavior: 'smooth'
                    });
                }
            }, 300);
            return;
        }

        // H3处理逻辑
        if (elementType === 'H3') {
            console.log('DEBUG 处理H3元素');
            // 查找父级H2并展开
            const parentTopic = actualTargetElement.closest('.topic');
            const parentH2 = parentTopic.find('h2.title').first();
            
            if (parentH2.length) {
                const parentBtn = parentH2.find('.wh_expand_btn');
                console.log('DEBUG 父级H2按钮:', parentBtn.length, '是否展开:', parentBtn.hasClass('expanded'));
                
                if (parentBtn.length && !parentBtn.hasClass('expanded')) {
                    parentBtn[0].click();
                    console.log('DEBUG 父级H2程序化展开');
                }
            }

            // 展开H3自身
            setTimeout(() => {
                const h3ExpandBtn = actualTargetElement.find('.wh_expand_btn');
                console.log('DEBUG H3按钮:', h3ExpandBtn.length, '是否展开:', h3ExpandBtn.hasClass('expanded'));
                
                if (h3ExpandBtn.length && !h3ExpandBtn.hasClass('expanded')) {
                    h3ExpandBtn[0].click();
                    console.log('DEBUG H3程序化展开');
                }
                
                // 滚动到目标位置 - 确保标题在视口顶部
                setTimeout(() => {
                    const targetOffset = actualTargetElement.offset();
                    if (targetOffset) {
                        const targetTop = targetOffset.top;
                        // 直接使用targetTop，因为它已经是从iframe顶部开始计算的偏移量
                        const finalPosition = Math.max(0, targetTop - 10); // 减去10像素确保完全可见，但不低于0
                        console.log('DEBUG H3精确滚动到位置:', finalPosition);
                        frameWindow.scrollTo({
                            top: finalPosition,
                            behavior: 'smooth'
                        });
                    }
                }, 250);
            }, 350);
            return;
        }

        // H4处理逻辑
        if (elementType === 'H4') {
            console.log('DEBUG 处理H4元素');
            // 查找父级H3并展开
            const parentSection = actualTargetElement.closest('.section');
            const parentH3 = parentSection.find('h3.sectiontitle, h3.title').first();
            
            if (parentH3.length) {
                const h3ParentBtn = parentH3.find('.wh_expand_btn');
                console.log('DEBUG 父级H3按钮:', h3ParentBtn.length, '是否展开:', h3ParentBtn.hasClass('expanded'));
                
                if (h3ParentBtn.length && !h3ParentBtn.hasClass('expanded')) {
                    h3ParentBtn[0].click();
                    console.log('DEBUG 父级H3程序化展开');
                }
            }

            // 展开H4自身
            setTimeout(() => {
                const h4ExpandBtn = actualTargetElement.find('.wh_expand_btn');
                console.log('DEBUG H4按钮:', h4ExpandBtn.length, '是否展开:', h4ExpandBtn.hasClass('expanded'));
                
                if (h4ExpandBtn.length && !h4ExpandBtn.hasClass('expanded')) {
                    h4ExpandBtn[0].click();
                    console.log('DEBUG H4程序化展开');
                }
                
                // 滚动到目标位置 - 确保标题在视口顶部
                setTimeout(() => {
                    const targetOffset = actualTargetElement.offset();
                    if (targetOffset) {
                        const targetTop = targetOffset.top;
                        // 直接使用targetTop，因为它已经是从iframe顶部开始计算的偏移量
                        const finalPosition = Math.max(0, targetTop - 10); // 减去10像素确保完全可见，但不低于0
                        console.log('DEBUG H4精确滚动到位置:', finalPosition);
                        frameWindow.scrollTo({
                            top: finalPosition,
                            behavior: 'smooth'
                        });
                    }
                }, 250);
            }, 350);
            return;
        }

        // 默认处理 - 直接滚动到目标位置
        setTimeout(() => {
            const targetOffset = actualTargetElement.offset();
            if (targetOffset) {
                const targetTop = targetOffset.top;
                // 直接使用targetTop，因为它已经是从iframe顶部开始计算的偏移量
                const finalPosition = Math.max(0, targetTop - 10); // 减去10像素确保完全可见，但不低于0
                console.log('DEBUG 默认滚动到位置:', finalPosition);
                frameWindow.scrollTo({
                    top: finalPosition,
                    behavior: 'smooth'
                });
            }
        }, 100);
    } else {
        console.log('DEBUG 未找到目标元素');
    }
}
// 热点图 显示 tip
function imgusemapTip(title, text){
	// 清空 目录导航
	$('.wt_footer_mobile_anchor_list').html("");
	// 显示
	$('.wt_footer_mobile_anchor_container').removeClass('hidden');
	// wt_footer_mobile_anchor_header > span  
	$('.wt_footer_mobile_anchor_header').children('span').eq(0).text(title);
	// wt_footer_mobile_anchor_list
                // alt 内容 分行处理
                var altArray = text.split('\\n');
	var divObject = $('<div></div>');
	for(var i=0; i< altArray.length; i++){
                         divObject.append("<div>" + altArray[i] + "</div>");
	}

	divObject.addClass('wt_img_usemap_text');
	$('.wt_footer_mobile_anchor_list').append(divObject);

}

// 目录：显示 mobile_menu_custom_tip
function menuCustomTipDisplayMobile(){
	if(IS_MENU_CUSTOM){
		$('.mobile_menu_custom_tip').show();
		// div_menu_header_ul 计算高度 
		$('#div_menu_header_ul').height("calc(100% - 18px - 66px - 48px - 46px)");
	} else {
		$('.mobile_menu_custom_tip').hide();
		// div_menu_header_ul 计算高度 
		$('#div_menu_header_ul').height("calc(100% - 18px - 66px - 48px)");
	}
}
