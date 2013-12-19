<?php
	
	$websites = $_GET['w'];
	$programs = $_GET['p'];
	$times = $_GET['t'];
	
	if($websites == ""){
		$websites = "null";
	}
	if($programs == ""){
		$programs = "null";
	}
	if($times == ""){
		$times = "0,0";
	}
	
	$stuff = "[main]\r\nblockedTimes=\"" . encrypt($times) . "\"\r\nwebsitesBlocked=\"" . encrypt($websites) . "\"\r\nprogramsBlocked=\"" . encrypt($programs) . "\"";
	
	file_put_contents ("CTConfig.ini",$stuff);
	
	function encrypt($string) {
        //Key 
        $key = "CTServic";
         
        //Encryption
        $cipher_alg = MCRYPT_TRIPLEDES;
    
        $iv = mcrypt_create_iv(mcrypt_get_iv_size($cipher_alg,MCRYPT_MODE_ECB), MCRYPT_RAND); 
         
 
        $encrypted_string = mcrypt_encrypt($cipher_alg, $key, $string, MCRYPT_MODE_ECB, $iv); 
        return base64_encode($encrypted_string);
        return $encrypted_string;
    }
	
?>