<?php
     
    /**
     * @author Markus Birth <markus@birth-online.de>
     * @link http://www.xenocafe.com/tutorials/php/ntp_time_synchronization/index.php
     * @link http://www.kloth.net/software/timesrv1.php
     */
     
    class NTP_TIME {
        protected static $port = 37;
        protected static $servers = array(
            'ptbtime1.ptb.de',
            'ptbtime2.ptb.de',
            'ntp0.fau.de',
            'ntps1-0.cs.tu-berlin.de',
            'ntps1-1.cs.tu-berlin.de',
            'ntps1-0.uni-erlangen.de',
            'ntp-p1.obspm.fr',
            'time.ien.it',
            'time.nrc.ca',
            'time.nist.gov',
            'nist1.datum.com',
            'time-a.timefreq.bldrdoc.gov',
            'utcnist.colorado.edu',
        );
     
        public static function query() {
            $got_time = false;
            foreach ( self::$servers as $ntpserver ) {
                $fp = @fsockopen( $ntpserver, 37, $errno, $errstr, 30 );
                if ( !$fp ) {
                    // offline or connection refused, try next
                    continue;
                }
                $data = '';
                while ( !feof( $fp ) ) {
                    $data .= fgets( $fp, 4 );
                }
                fclose( $fp );
     
                if ( strlen( $data ) == 4 ) {
                    $got_time = true;
                    break;
                }
            }
     
            if ( !$got_time ) return 0;
     
            $time1900 = hexdec( bin2hex( $data ) );
            $timestamp = $time1900 - 2208988800;   // Time server is based on 1900 while Unix is based on 1970
     
            return $timestamp;
        }
    }
     
?>

