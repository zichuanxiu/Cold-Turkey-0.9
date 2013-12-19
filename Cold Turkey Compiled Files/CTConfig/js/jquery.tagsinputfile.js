/*

	jQuery Tags Input Plugin 1.3.3
	
	Modifications made for file input for Cold Turkey
	
	Copyright (c) 2011 XOXCO, Inc (c) 2013 Felix Belzile
	
	Documentation for this plugin lives here:
	http://xoxco.com/clickable/jquery-tags-input
	
	Licensed under the MIT license:
	http://www.opensource.org/licenses/mit-license.php

	ben@xoxco.com

*/

(function($) {

	var delimiter = new Array();
	var tags_callbacks = new Array();
	$.fn.doAutosizeFile = function(o){
    var minWidth = $(this).data('minwidth'),
          maxWidth = $(this).data('maxwidth'),
          val = '',
          input = $(this),
          testSubject = $('#'+$(this).data('tester_id'));
	
      if (val === (val = input.val())) {return;}
	
      var escaped = val.replace(/&/g, '&amp;').replace(/\s/g,' ').replace(/</g, '&lt;').replace(/>/g, '&gt;');
      testSubject.html(escaped);
      // Calculate new width + whether to change
      var testerWidth = testSubject.width(),
          newWidth = (testerWidth + o.comfortZone) >= minWidth ? testerWidth + o.comfortZone : minWidth,
          currentWidth = input.width(),
          isValidWidthChange = (newWidth < currentWidth && newWidth >= minWidth) || (newWidth > minWidth && newWidth < maxWidth);

      if (isValidWidthChange) {
          input.width(newWidth);
      }


  };
  $.fn.resetAutosizeFile = function(options){
    // alert(JSON.stringify(options));
    var minWidth =  $(this).data('minwidth') || options.minInputWidth || $(this).width(),
        maxWidth = $(this).data('maxwidth') || options.maxInputWidth || ($(this).closest('.tagsinput').width() - options.inputPadding),
        val = '',
        input = $(this),
        testSubject = $('<tester/>').css({
            position: 'absolute',
            top: -9999,
            left: -9999,
            width: 'auto',
            fontSize: input.css('fontSize'),
            fontFamily: input.css('fontFamily'),
            fontWeight: input.css('fontWeight'),
            letterSpacing: input.css('letterSpacing'),
            whiteSpace: 'nowrap'
        }),
        testerId = $(this).attr('id')+'_autosize_tester';
    if($('#'+testerId).length < 1){
      testSubject.attr('id', testerId);
      testSubject.appendTo('body');
    }

    input.data('minwidth', minWidth);
    input.data('maxwidth', maxWidth);
    input.data('tester_id', testerId);
    input.css('width', minWidth);
  };
  
	$.fn.addTagFile = function(value,options) {
			options = jQuery.extend({focus:false,callback:true},options);
			this.each(function() { 
				var id = $(this).attr('id');
				var lockedF = false;
        var skipTag = $(this).tagExistFile(value);
				var tagslist = $(this).val().split(delimiter[id]);
				if (tagslist[0] === '') { 
					tagslist = new Array();
				}

				value = jQuery.trim(value);
				
				if (value.indexOf("!") === 0){
					lockedF = true;
				}
				
				if (options.unique) {
					if(skipTag === true) {
                $('#'+id+'_tag').addClass('not_valid');
          }
				}
				
				if (value !=='' && skipTag !== true) { 
				
          if (lockedF === true){
						$('<span>').addClass('tag').append($('<span>').text(value).append('&nbsp;&nbsp;')).insertBefore('#' + id + '_addTag');
					}else{
						$('<span>').addClass('tag').append(
							$('<span>').text(value).append('&nbsp;&nbsp;'),
							$('<a>', {
								href  : '#',
								title : 'Remove tag',
								text  : 'x'
							}).click(function () {
								return $('#' + id).removeTagFile(escape(value));
							})
						).insertBefore('#' + id + '_addTag');
					}

					tagslist.push(value);
				
					$('#'+id+'_tag').val('');
					if (options.focus) {
						$('#'+id+'_tag').focus();
					} else {		
						$('#'+id+'_tag').blur();
					}
					
					$.fn.tagsInputFile.updateTagsFieldFile(this,tagslist);
					
					if (options.callback && tags_callbacks[id] && tags_callbacks[id]['onAddTag']) {
						var f = tags_callbacks[id]['onAddTag'];
						f.call(this, value);
					}
					if(tags_callbacks[id] && tags_callbacks[id]['onChangeFile'])
					{
						var i = tagslist.length;
						var f = tags_callbacks[id]['onChangeFile'];
						f.call(this, $(this), tagslist[i-1]);
					}					
				}
			});		
			return false;
		};
		
	$.fn.removeTagFile = function(value) { 
			value = unescape(value);
			this.each(function() { 
				var id = $(this).attr('id');
	
				var old = $(this).val().split(delimiter[id]);
					
				$('#'+id+'_tagsinput .tag').remove();
				str = '';
				for (i=0; i< old.length; i++) { 
					if (old[i]!=value) { 
						str = str + delimiter[id] +old[i];
					}
				}
				
				$.fn.tagsInputFile.importTagsFile(this,str);

				if (tags_callbacks[id] && tags_callbacks[id]['onRemoveTag']) {
					var f = tags_callbacks[id]['onRemoveTag'];
					f.call(this, value);
				}
			});
					
			return false;
		};
	
	$.fn.tagExistFile = function(val) {
		var id = $(this).attr('id');
		var tagslist = $(this).val().split(delimiter[id]);
		return (jQuery.inArray(val, tagslist) >= 0); //true when tag exists, false when not
	};
	
	// clear all existing tags and import new ones from a string
	$.fn.importTagsFile = function(str) {
                id = $(this).attr('id');
		$('#'+id+'_tagsinput .tag').remove();
		$.fn.tagsInputFile.importTagsFile(this,str);
	};
		
	$.fn.tagsInputFile = function(options) { 
    var settings = jQuery.extend({
      interactive:true,
      defaultText:'Add something...',
      minChars:0,
      width:'100%',
      height:'100px',
      autocomplete: {selectFirst: false },
      'hide':true,
      'delimiter':',',
      'unique':true,
      removeWithBackspace:true,
      placeholderColor:'#666666',
      autosize: true,
      comfortZone: 20,
      inputPadding: 6*2
    },options);

		this.each(function() { 
			if (settings.hide) { 
				$(this).hide();				
			}
			var id = $(this).attr('id');
			if (!id || delimiter[$(this).attr('id')]) {
				id = $(this).attr('id', 'tags' + new Date().getTime()).attr('id');
			}
			
			var data = jQuery.extend({
				pid:id,
				real_input: '#'+id,
				holder: '#'+id+'_tagsinput',
				input_wrapper: '#'+id+'_addTag',
				fake_input: '#'+id+'_tag'
			},settings);
	
			delimiter[id] = data.delimiter;
			
			if (settings.onAddTag || settings.onRemoveTagFile || settings.onChangeFile) {
				tags_callbacks[id] = new Array();
				tags_callbacks[id]['onAddTag'] = settings.onAddTagFile;
				tags_callbacks[id]['onRemoveTag'] = settings.onRemoveTagFile;
				tags_callbacks[id]['onChangeFile'] = settings.onChangeFile;
			}
	
			var markup = '<div id="'+id+'_tagsinput" class="tagsinput"><div id="'+id+'_addTag">';
			
			if (settings.interactive) {
				markup = markup + '<input id="'+id+'_tag" value="" data-default="'+settings.defaultText+'" />';
			}
			
			markup = markup + '</div><div class="tags_clear"></div></div>';
			
			$(markup).insertAfter(this);

			$(data.holder).css('width',settings.width);
			$(data.holder).css('min-height',settings.height);
			$(data.holder).css('height',settings.height);
	
			if ($(data.real_input).val()!=='') { 
				$.fn.tagsInputFile.importTagsFile($(data.real_input),$(data.real_input).val());
			}		
			if (settings.interactive) {
				var eventPassed;
				$(data.fake_input).val($(data.fake_input).attr('data-default'));
				$(data.fake_input).css('color',settings.placeholderColor);
				$(data.fake_input).resetAutosizeFile(settings);
				
				$(data.holder).bind('click',data,function(event) {
					eventPassed = event;
					document.getElementById('fileID').click();
				});
				$(document).on('change', '#fileID', function() {
					if ($("#fileID").val() == ""){
						return false;
					}else{
						$(eventPassed.data.fake_input).val($("#fileID").val())
						$(eventPassed.data.fake_input).focus();
					}
				});

				$(data.fake_input).bind('focus',data,function(event) {
				event = eventPassed;
					if ($(event.data.fake_input).val()==$(event.data.fake_input).attr('data-default')) { 
						$(event.data.fake_input).val('');
					}else{
						$(event.data.fake_input).css('color','#000000');
					}
					document.getElementById('fileID').value = '';
					$(event.data.fake_input).blur();
				});		
				if (settings.autocomplete_url !== undefined) {
					autocomplete_options = {source: settings.autocomplete_url};
					for (var attrname in settings.autocomplete) { 
						autocomplete_options[attrname] = settings.autocomplete[attrname]; 
					}
				
					if (jQuery.Autocompleter !== undefined) {
						$(data.fake_input).autocomplete(settings.autocomplete_url, settings.autocomplete);
						$(data.fake_input).bind('result',data,function(event,data,formatted) {
							if (data) {
								$('#'+id).addTagFile(data[0] + "",{focus:true,unique:(settings.unique)});
							}
              });
					} else if (jQuery.ui.autocomplete !== undefined) {
						$(data.fake_input).autocomplete(autocomplete_options);
						$(data.fake_input).bind('autocompleteselect',data,function(event,ui) {
							$(event.data.real_input).addTagFile(ui.item.value,{focus:true,unique:(settings.unique)});
							return false;
						});
					}
				
					
				} else {
						// if a user tabs out of the field, create a new tag
						// this is only available if autocomplete is not used.
						$(data.fake_input).bind('blur',data,function(event) { 
							var d = $(this).attr('data-default');
							if ($(event.data.fake_input).val()!=='' && $(event.data.fake_input).val()!==d) {
								if( (event.data.minChars <= $(event.data.fake_input).val().length) && (!event.data.maxChars || (event.data.maxChars >= $(event.data.fake_input).val().length)) )
									$(event.data.real_input).addTagFile($(event.data.fake_input).val(),{focus:true,unique:(settings.unique)});
									
							} else {
								$(event.data.fake_input).val($(event.data.fake_input).attr('data-default'));
								$(event.data.fake_input).css('color',settings.placeholderColor);

								if ($(this).hasClass('not_valid') === true) {
								$(this).removeClass('not_valid');
								}
							}
							return false;
						});
				
				}
				/* if user types a comma, create a new tag
				$(data.fake_input).bind('keypress',data,function(event) {
					if (event.which==event.data.delimiter.charCodeAt(0) || event.which==13 ) {
					    event.preventDefault();
						if( (event.data.minChars <= $(event.data.fake_input).val().length) && (!event.data.maxChars || (event.data.maxChars >= $(event.data.fake_input).val().length)) )
							$(event.data.real_input).addTagFile($(event.data.fake_input).val(),{focus:true,unique:(settings.unique)});
					  	$(event.data.fake_input).resetAutosizeFile(settings);
						return false;
					} else if (event.data.autosize) {
			            $(event.data.fake_input).doAutosizeFile(settings);
            
          			}
				});
				Delete last tag on backspace
				data.removeWithBackspace && $(data.fake_input).bind('keydown', function(event)
				{
					if(event.keyCode == 8 && $(this).val() == '')
					{
						 event.preventDefault();
						 var last_tag = $(this).closest('.tagsinput').find('.tag:last').text();
						 var id = $(this).attr('id').replace(/_tag$/, '');
						 last_tag = last_tag.replace(/[\s]+x$/, '');
						 $('#' + id).removeTagFile(escape(last_tag));
						 $(this).trigger('focus');
					}
				}); */
				if(data.unique) {
				    $(data.fake_input).keydown(function(event){
				        event.preventDefault();
						return false;
				    });
				}
				$(data.fake_input).blur();
			} // if settings.interactive
		});
			
		return this;
	
	};

	$.fn.tagsInputFile.updateTagsFieldFile = function(obj,tagslist) { 
		var id = $(obj).attr('id');
		$(obj).val(tagslist.join(delimiter[id]));
	};
	
	$.fn.tagsInputFile.importTagsFile = function(obj,val) {			
		$(obj).val('');
		var id = $(obj).attr('id');
		var tags = val.split(delimiter[id]);
		for (i=0; i<tags.length; i++) { 
			$(obj).addTagFile(tags[i],{focus:false,callback:false});
		}
		if(tags_callbacks[id] && tags_callbacks[id]['onChangeFile'])
		{
			var f = tags_callbacks[id]['onChangeFile'];
			f.call(obj, obj, tags[i]);
		}
	};

})(jQuery);
