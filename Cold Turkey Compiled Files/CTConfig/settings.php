<!DOCTYPE html>
<html dir="ltr" lang="en-US" class="no-js">
	<head>

		<!-- ==============================================
		Title and basic Metas
		=============================================== -->
		<meta charset="utf-8">
		<title>Cold Turkey - Configuration</title>
		<meta name="description" content="Cold Turkey Configuration">
		<meta name="author" content="Felix Belzile">

		<!-- ==============================================
		Mobile Metas
		=============================================== -->
		<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1">

		<!-- ==============================================
		CSS
		=============================================== -->
		<link rel="stylesheet" href="css/bootstrap.min.css" /> 
		<link rel="stylesheet" href="css/bootstrap-responsive.min.css" />
		<link rel="stylesheet" href="css/font-awesome.min.css">
		<link rel="stylesheet" href="css/flexslider.css"> 
		<link rel="stylesheet" href="css/style.css">
		<link rel="stylesheet" href="css/responsive.css">
		<link rel="stylesheet" href="css/jquery.tagsinput.css"> 
		<style type="text/css">#intro {background: url(../images/img<?php echo rand(1,6); ?>.jpg) no-repeat center center fixed;</style>
		<!--[if IE 7]>
			<link rel="stylesheet" href="css/font-awesome-ie7.min.css">
		<![endif]-->
		
		<!--[if lt IE 9]>
			<script src="http://html5shim.googlecode.com/svn/trunk/html5.js"></script>
		<![endif]-->

		<!-- ==============================================
		Fonts
		=============================================== -->
		<link href='http://fonts.googleapis.com/css?family=Open+Sans:400,300|Open+Sans+Condensed:300' rel='stylesheet' type='text/css'>

		<!-- ==============================================
		JS
		=============================================== -->
		<script type="text/javascript" src="js/modernizr.js"></script> <!-- Modernizr file -->			

		<!-- ==============================================
		Favicons
		=============================================== -->
		<link rel="shortcut icon" href="images/favicon.ico">
		
	</head>
	<body>
		
		<!-- ==============================================
		Preloader
		=============================================== -->
		<div id="preloader">
    		<div id="loading-animation">&nbsp;</div>
		</div>
		
		<!-- ==============================================
		Navigation
		=============================================== -->
		<nav id="main-nav">
			
			<ul>
		    	<li><a href="#intro">Intro</a></li>
		    	<li><a href="#what">What</a></li>
		    	<li><a href="#when">When</a></li>
		    	<li><a href="#go">Let's Go!</a></li>
			</ul>

			<a href="#what" class="logo content-menu-link">
				<img src="images/logo.png" alt="">
			</a>

			<a href="#" id="responsive-nav">
				<i class="icon-list"></i>
			</a>
			
		</nav>			
		
		<!-- ==============================================
		Intro
		=============================================== -->
		<section id="intro" class="section">
			<?php
				$connectionSuccess = true;
				try{
					if (floatVal(@file_get_contents('http://getcoldturkey.com/version.html')) > 0.9) {
						echo "<div id=\"update\">This version of Cold Turkey is out of date. <a href=\"http://getcoldturkey.com/download/\">Install the new version now!</a></div>";
					}
				} catch (Exception $e) {
				}
				
			?>
			<div class="container">
				<div class="row">
					<?php
						include(".\class.iniparser.php");
						include(".\class.world.time.php");
						try {
  
							$locked = false;
							$timezone = ($_COOKIE['offset'])*-60;
							$iniFile = new iniParser(".\CTConfig.ini");
							$newWorldTime = new NTP_TIME();
							$time = $newWorldTime->query();	
							date_default_timezone_set('UTC');

							$blockedTime = $iniFile->get("main","blockedTimes");
							$decrypted= ($this).decrypt($blockedTime);
							$blockedTimes = explode("," , $decrypted);
							sort($blockedTimes);
							//Unix time 0 plus or minus 24 hours to play safe
							if ($time > 180000){
								if ($time > end($blockedTimes)){
									echo "<h1>Let's get some work <span>done</span></h1><h2>This guide will help set up your block.<br>Don't worry, the block won't start until you click \"Go Cold Turkey\"</h2>";
								}else{
									$locked = true;
									echo "<h1>Some settings are <span>locked</span></h1><h2>You will not be able to remove restrictions until<br>" . date('l \t\h\e jS \a\t h:i A', (end($blockedTimes) + $timezone)) . "</h2>";
								}
							}else{
								echo "<h1>Houston, we have a problem</h1><h2>Looks like we couldn't connect to the time server. Try to add exceptions<br>to your firewall to allow all CT executables to access the Internet.</h2>";
							}
						} catch (Exception $e) {
						}
						function decrypt($string) {
							try{
								$string = base64_decode($string);
					 
								$key = "CTServic";
								 
								$cipher_alg = MCRYPT_TRIPLEDES;
					 
								$iv = mcrypt_create_iv(mcrypt_get_iv_size($cipher_alg,MCRYPT_MODE_ECB), MCRYPT_RAND); 
								 
								$decrypted_string = mcrypt_decrypt($cipher_alg, $key, $string, MCRYPT_MODE_ECB, $iv); 
								return trim($decrypted_string);
							} catch (Exception $e) {
								echo '';
							}
						}
						if($time > 180000){
							echo '<a class="get-started content-menu-link" href="#what">Next <i class="icon-angle-right"></i></a>';
						}
					?>
				</div>
			</div>
			
		</section>
		
	
		<!-- ==============================================
		what
		=============================================== -->
		<section id="what" class="section">
			<div class="container">
				<div class="row">
					<div class="span12">
						<h1>Block What?</h1>
						<hr class="fancy-hr">
						<div class="row">
							<div class="span12">
								<h3>Websites</h3>
								<p><input id="tag1" value=
								<?php
									$websitesBlocked = ($this).decrypt($iniFile->get("main","websitesBlocked"));
									$websitesBlockedArray = explode("," , $websitesBlocked);
									if ($locked){
										if (strlen($websitesBlocked) < 5) {
											echo "facebook.com,twitter.com,myspace.com,reddit.com,stumbleupon.com,9gag.com,imgur.com,addictinggames.com,collegehumor.com,failblog.org";
										}else {
											foreach ($websitesBlockedArray as $site) {
												echo '!' . $site . ',';
											}
										}
									}
									else{
										if (strlen($websitesBlocked) < 5) {
											echo "facebook.com,twitter.com,myspace.com,reddit.com,stumbleupon.com,9gag.com,imgur.com,addictinggames.com,collegehumor.com,failblog.org";
										}else{
											echo $websitesBlocked;
										}
									}
								?> /></p>
								<h3>Applications</h3>
								<div id="hideText"><input type="file" accept=".exe,application/octet-stream" name="fileID" id="fileID" style="visibility: hidden;" /></div>
								<p><input id="tag2" 
								<?php
									$programsBlocked = ($this).decrypt($iniFile->get("main","programsBlocked"));
									$programsBlockedArray = explode("," , $programsBlocked);
									if ($locked){
										if (strlen($programsBlocked) > 4) {
											echo "value=\"";
											foreach ($programsBlockedArray as $prog) {
												echo '!' . $prog . ',';
											}
											echo "\"";
										}
									}
									else{
										if (strlen($programsBlocked) > 4) {
											echo "value=\"" . $programsBlocked . "\"";
										}
									}
								?>							
								></p>
								<a class="aboutNav content-menu-link" href="#when">Next <i class="icon-angle-right"></i></a><div>&nbsp;</div>
							</div> <!-- End Span8 -->
						</div> <!-- End Row -->
					</div> <!-- End Span12 -->
				</div> <!-- End Row -->
			</div>	
		</section>

		<!-- ==============================================
		when
		=============================================== -->
		<section id="when" class="section">
			<div class="container">
				<div class="row">
					<div class="span12">
						<h1>Block When?</h1>
						<hr class="fancy-hr">
					</div>
					<div id="days">
						<table>
						<tr>
							<td class="daysClassTime"></td>
							<td class="daysClass">Today</td>
							<td class="daysClass"><?php echo substr(date('l', strtotime("midnight", $time+$timezone+86400)),0,3); ?></td>
							<td class="daysClass"><?php echo substr(date('l', strtotime("midnight", $time+$timezone+2*86400)),0,3); ?></td>
							<td class="daysClass"><?php echo substr(date('l', strtotime("midnight", $time+$timezone+3*86400)),0,3); ?></td>
							<td class="daysClass"><?php echo substr(date('l', strtotime("midnight", $time+$timezone+4*86400)),0,3); ?></td>
							<td class="daysClass"><?php echo substr(date('l', strtotime("midnight", $time+$timezone+5*86400)),0,3); ?></td>
							<td class="daysClass"><?php echo substr(date('l', strtotime("midnight", $time+$timezone+6*86400)),0,3); ?></td>
						</tr>
						</table>
					</div>
					<div id="scrollit">
						<table class="time">
						<tr>
						   <td class="time">midnight</td>
						</tr>
						<tr>
						   <td class="time"><span>(1am)</span> 01:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(2am)</span> 02:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(3am)</span> 03:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(4am)</span> 04:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(5am)</span> 05:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(6am)</span> 06:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(7am)</span> 07:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(8am)</span> 08:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(9am)</span> 09:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(10am)</span> 10:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(11am)</span> 11:00</td>
						</tr>
						<tr>
						   <td class="time">noon</td>
						</tr>
						<tr>
						   <td class="time"><span>(1pm)</span> 13:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(2pm)</span> 14:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(3pm)</span> 15:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(4pm)</span> 16:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(5pm)</span> 17:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(6pm)</span> 18:00</td>
						 </tr>
						<tr>
						   <td class="time"><span>(7pm)</span> 19:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(8pm)</span> 20:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(9pm)</span> 21:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(10pm)</span> 22:00</td>
						</tr>
						<tr>
						   <td class="time"><span>(11pm)</span> 23:00</td>
						</tr>
						</table>
						
						<?php 
							$lockedArray = array();
							for($j=0; $j<=(count($blockedTimes)/2); $j++){
								for($i=$blockedTimes[$j*2]; $i<$blockedTimes[($j*2)+1]; $i+=1800){
									$lockedArray[] = $i;
								}
							}
						?>
						
						<table cellpadding="0" cellspacing="0" id="our_table">
						<tr>
						   <td <?php $midnight = strtotime(date('Y-m-d',$time+$timezone).' 00:00:00')-$timezone; if (in_array($midnight, $lockedArray) && $locked){ echo "id=\"" . $midnight . "\" class=\"locked\">locked"; }else{echo "id=\"" . $midnight . "\">";}?></td>
						   <td <?php $day1 = $midnight+86400; if (in_array($day1, $lockedArray) && $locked){ echo "id=\"" . $day1 . "\" class=\"locked\">locked"; }else{echo "id=\"" . $day1 . "\">";}?></td>
						   <td <?php $day2 = $midnight+2*86400; if (in_array($day2, $lockedArray) && $locked){ echo "id=\"" . $day2 . "\" class=\"locked\">locked"; }else{echo "id=\"" . $day2 . "\">";}?></td>
						   <td <?php $day3 = $midnight+3*86400; if (in_array($day3, $lockedArray) && $locked){ echo "id=\"" . $day3 . "\" class=\"locked\">locked"; }else{echo "id=\"" . $day3 . "\">";}?></td>
						   <td <?php $day4 = $midnight+4*86400; if (in_array($day4, $lockedArray) && $locked){ echo "id=\"" . $day4 . "\" class=\"locked\">locked"; }else{echo "id=\"" . $day4 . "\">";}?></td>
						   <td <?php $day5 = $midnight+5*86400; if (in_array($day5, $lockedArray) && $locked){ echo "id=\"" . $day5 . "\" class=\"locked\">locked"; }else{echo "id=\"" . $day5 . "\">";}?></td>
						   <td <?php $day6 = $midnight+6*86400; if (in_array($day6, $lockedArray) && $locked){ echo "id=\"" . $day6 . "\" class=\"locked\">locked"; }else{echo "id=\"" . $day6 . "\">";}?></td>
						</tr>
						
						<?php
							for ($m=1; $m<48; $m++){
								echo "<tr>";
								for ($k=0; $k<7; $k++){
									$id = $midnight+($k*86400)+($m*1800);
									if(in_array($id, $lockedArray) && $locked){
										echo "<td id=\"" . $id . "\" class=\"locked\">locked</td>";
									} else{
										echo "<td id=\"" . $id . "\"></td>";
									}
								}
								echo "</tr>";
							}
						?>
						
						</table>
					</div>
					<div id="legend">Select: <a onClick="selectAll();">Everything</a> | <a onClick="unSelectAll();">Nothing</a></div>
					<div id="servicesNavMenu">
						<a class="servicesNav content-menu-link" href="#what">Back <i class="icon-angle-left"></i></a>
						<a class="servicesNav content-menu-link" href="#go">Next <i class="icon-angle-right"></i></a>
					</div><div>&nbsp;</div>
				</div> <!-- End Row Features -->
			</div> <!-- End Row -->

		</section>

		<!-- ==============================================
		go
		=============================================== -->
		<section id="go" class="section">
			<div class="container">
				<div class="row">
					<div class="span12">
						<h1>Don't chicken out...</h1>
						<hr class="fancy-hr">
						<h2>Make sure the information you entered is correct.</h2><h3>You can not undo these changes until the <u>last</u> block period is over. However, you can always add more time and things to block.
						You can see how much time you have left by running Cold Turkey from your desktop, or by going to: <a target="_blank" href="http://getcoldturkey.com">http://getcoldturkey.com</a></h3>
						<hr class="fancy-hr">
						<div id="servicesNavMenu">
							<a class="servicesNav content-menu-link" href="#when">Back <i class="icon-angle-left"></i></a>
							<a class="servicesNav content-menu-link" onclick="startBlock();" href="javascript:void(0)">Go Cold Turkey!</a>
						</div>
						<div>&nbsp;</div>
					</div>
				</div>
			</div>
			<iframe id="sneakyFrame" style="display: none"></iframe>
		</section>
		
		<!-- ==============================================
		start
		=============================================== -->
		<section id="start" class="section">
			<div class="container">
				<div class="row">
					<div class="span12">
						<h1>Block Successful</h1>
						<hr class="fancy-hr">
						<h2>Now, get to work!</h2>
						<hr class="fancy-hr">
						<div id="servicesNavMenu">
							<a class="servicesNav content-menu-link" onclick="window.close();" href="javascript:void(0)">Close</a><br>
						</div>
					</div>
				</div>
			</div>
		</section>
		<!-- ==============================================
		JS
		=============================================== -->
		
		<script type="text/javascript" src="js/jquery.min.js"></script> 
		<script type="text/javascript" src="js/bootstrap.min.js"></script>
		<script type="text/javascript" src="js/jquery.easing.pack.js"></script> 
		<script type="text/javascript" src="js/jquery.mousewheel.pack.js"></script> 
		<script type="text/javascript" src="js/jquery.fancybox.pack.js"></script> 
		<script type="text/javascript" src="js/jquery.flexslider.min.js"></script>
		<script type="text/javascript" src="js/jquery.isotope.min.js"></script>
		<script type="text/javascript" src="js/jquery.validate.min.js"></script>
		<script type="text/javascript" src="js/jquery.tagsinput.js"></script>
		<script type="text/javascript" src="js/jquery.tagsinputfile.js"></script>
		<script type="text/javascript">
			$(function() {
				var isMouseDown = false,
				isHighlighted;
								
				$('#tag2').tagsInputFile({});
				$('#tag1').tagsInput({});
				$('#our_table td')
					.mousedown(function () {
						if(!$(this).hasClass("locked")){
							isMouseDown = true;
							$(this).toggleClass("highlighted");
							isHighlighted = $(this).hasClass("highlighted");
							if (isHighlighted) {
								$(this).html("block");
							} else {
								$(this).html("");
							}
						}
						
						return false;
					})
					.mouseover(function () {
						if (isMouseDown) {
							if(!$(this).hasClass("locked")){
								$(this).toggleClass("highlighted", isHighlighted);
								if (isHighlighted) {
									$(this).html("block");
								} else {
									$(this).html("");
								}
							}
						}
					})
					.bind("selectstart", function () {
						return false;
					})

				$(document)
					.mouseup(function () {
						isMouseDown = false;
					});
				$(document).ready(function() {

					var autoscroll = document.getElementById("scrollit");
					autoscroll.scrollTop = 320;
				
					window.startBlock = function(){
						var idArray = [];
						var suffixStart = "start.php";
						var suffix = "", w = "", p = "", t = "";
						
						w = document.getElementById('tag1').value
						w = w.replace(/!/g, '');
						
						p = document.getElementById('tag2').value
						p = p.replace(/!/g, '');
						
						$('.highlighted').each(function () {
							idArray.push(this.id);
						});
						$('.locked').each(function () {
							idArray.push(this.id);
						});
						
						if(idArray.length < 1){
							alert("You need to select at least one time period to block.");
							return false;
						}
						if(w.length < 4 && p.length < 3){
							alert("You need to block at least one website or one program.");
							return false;
						}
						
						idArray.sort();
						t = t.concat(idArray[0], ',');
						for (var i = 1; i < idArray.length; i++){
							if ((idArray[i] - idArray[i-1]) > 1800){
								t = t.concat((parseInt(idArray[i-1]) + 1800), ',' , idArray[i] , ',');
							}
						}
						if((parseInt(idArray[idArray.length-1]) + 1800) < <?php echo $time;?>){
							alert("The block times you selected have already went by. \nMake sure you selected the correct times.");
							return false;
						}
						
						t = t.concat(parseInt(idArray[idArray.length-1]) + 1800);
						
						suffix = suffixStart.concat('?w=', w, '&p=', p, '&t=', t);
						$('#sneakyFrame').attr('src', suffix);
						window.location.assign("http://localhost:1990/settings.php#start")
						return false;						
					}
					window.unSelectAll = function() {
						$('.highlighted').each(function () {
							if(!$(this).hasClass("locked")){
								$(this).toggleClass("highlighted");
								$(this).html("");
							}
						});
					}
					window.selectAll = function() {
						$('#our_table td').each(function () {
							if(!$(this).hasClass("locked")){
								$(this).toggleClass("highlighted", true);
								$(this).html("block");
							}
						});
					}
				});

			});

		</script>
		<script type="text/javascript" src="js/reversal.js"></script>	

	</body>
</html>
