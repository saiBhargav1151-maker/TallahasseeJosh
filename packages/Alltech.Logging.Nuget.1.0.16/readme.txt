Alltech Logging is a package to enable url analytics, error logging, and post logging

Instructions:
1) First change your web config settings
2) Implement the method GetAdditionalInformation() in App_Start\LoggingBootstrap
3) Comment out the default proxy for use not on Fdot Vpn in web.config
4) Add javascript references:
	<script type="text/javascript" src="./Scripts/stacktrace.js"></script>
    <script type="text/javascript" src="./Scripts/alltech-logging.js"></script>
5) Add 'alltech.logging' to your list of angular module dependencies