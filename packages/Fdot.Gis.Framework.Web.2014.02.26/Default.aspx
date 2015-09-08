<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FDOT.GIS.Web.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>FDOT GIS Framework - <%= FDOT.GIS.Web.ApplicationContext.Current.Configuration.Title %></title>
    <style type="text/css">
        html, body, form
        {
            height: 100%;
            width: 100%;
            overflow: hidden;
            font-family: Arial, Helvetica, Verdana;
        }
        body
        {
            padding: 0;
            margin: 0;
        }
        #silverlightControlHost
        {
            height: 100%;
            text-align: center;            
        }
        #footer
        {
           	height: 60px;
        	width: 100%;
        	clear: both;
        	border-top: 2px solid black;
        }
        #footer-left-content
        {
        	width: 15%;
        	height: 100%;
        	float: left;
        	padding-left: 5px;
        	padding-top: 5px;
        }
        #footer-center-content
        {
        	width: 68%;
        	height: 100%;
        	float: left;
        	text-align: center;
        	vertical-align: middle;
        	padding-left: 5px;
        	padding-top: 5px;
        }
        #footer-right-content
        {
        	width: 15%;
        	height: 100%;
        	float: right; 
        	padding-right: 5px;
        	padding-top: 5px;       	
        }
        #closefooter
        {       	
        	float: right;
        	width: 13px;
        	height: 13px;
        }
        .error-message
        {
            color: Red;
            font-weight: bold;
        }
        .error-detail
        {  
            text-align: left;            
            margin: auto;      
            white-space: pre-wrap;             
        }
    </style>
</head>
<body>
    <noscript>We have detected that Javascript execution is currently disabled in your browser. Javascript must be enabled in order to use this site.</noscript>
    <form id="form1" runat="server" >   
        <div id="silverlightControlHost">
            <iframe id="_sl_historyFrame" style="visibility: hidden; height: 0px; width: 0px; border: 0px"></iframe>
        </div>    
        <div id='footer'>
            <div id='footer-left-content'>
                <img src='http://www2.dot.state.fl.us/images/oislogo_sm.jpg' alt='FDOT Office of Information Systems logo' />
            </div>
            <div id='footer-center-content'>
                FLORIDA DEPARTMENT OF TRANSPORTATION<br />
                Report Issues to the Service Desk @ 1-866-955-4357 (HELP) or e-mail: <a href='mailto:FDOT.ServiceDesk@dot.state.fl.us?subject=GIS Framework Issue'>Service Desk</a>
            </div>
            <div id='footer-right-content'></div>
        </div>
    </form>
    <script type="text/javascript" src="Scripts/all.js"></script>
    <script type="text/javascript">

        function onExtentChange(s, a) {
            var geometry = {
                XMin: a.XMin,
                XMax: a.XMax,
                YMin: a.YMin,
                YMax: a.YMax
            };
            
            var currentState = getStateFromUrl();
            currentState.extent = geometry;
            putStateinUrl(currentState);
        }

        function onLayersChange(s, a) {
            var currentState = getStateFromUrl();
            currentState.activeLayers = new Array();
            var x = {};
            for (var i = 0; i < a.LayerId.length; i++) {
                
                var ids = x[a.MapServiceName[i]];
                if (ids === undefined || ids === null) {
                    x[a.MapServiceName[i]] = [];                  
                }

                x[a.MapServiceName[i]].push(a.LayerId[i]);
            }
            currentState.activeLayers = x;
            putStateinUrl(currentState);
           
        }

        function getStateFromUrl() {
            var hash = window.location.hash;
            if (hash == null || hash == '') return {};

            var state = JSON.parse(decodeURIComponent(hash.substring(1)));
            return state;
        }

        function putStateinUrl(state) {
            window.location.hash = JSON.stringify(state);
        }

        function setFooterVisibility(isVisible) {
            if (isVisible !== null && isVisible.toLowerCase() === 'false') {
                $('#footer').hide();
                $('#silverlightControlHost').height('100%');
                FDOT.Map.setProperty("Footer.IsVisible", "false");
            }
            else {
                $('#silverlightControlHost').height($(document).height() - $('#footer').height());
                $('#footer').show();
                FDOT.Map.setProperty("Footer.IsVisible", "true");
            }
        }

        $(function () {
            $('#silverlightControlHost')
                .height($(document).height() - $('#footer').height());

            $('<img id="closefooter" alt="Close the page footer" src="images/closefooter.jpg" />')
                .click(function () {
                    setFooterVisibility('false')
                })
                .appendTo('#footer-right-content');


            var urlParams = {};
            (function () {
                var e,
                    a = /\+/g,  // Regex for replacing addition symbol with a space
                    r = /([^&=]+)=?([^&]*)/g,
                    d = function (s) { return decodeURIComponent(s.replace(a, " ")); },
                    q = window.location.search.substring(1);

                while (e = r.exec(q))
                    urlParams[d(e[1])] = d(e[2]);
            })();

            var initialExtent = '';
            var x = {};
            var currentState = getStateFromUrl();

            if (currentState && currentState.extent) {
                initialExtent = '' + currentState.extent.XMin + '|' + currentState.extent.YMin + '|' + currentState.extent.XMax + '|' + currentState.extent.YMax;
            }

            if (currentState && currentState.activeLayers) {
                var stateString = '';
                for (var i in currentState.activeLayers) {
                    if (stateString !== '') {
                        stateString = stateString + '~';
                    }

                    stateString = stateString + i;
                    for (var layerIndex = 0; layerIndex < currentState.activeLayers[i].length; layerIndex++) {
                        stateString = stateString + '|' + currentState.activeLayers[i][layerIndex];
                    }
                }
            }
            FDOT.Map.create({
                containerSelector: '#silverlightControlHost',
                id: 'slctl',
                appId: urlParams.appId,
                initialExtent: initialExtent,
                activeLayerInfo: stateString,
                onReady: function () {
                    FDOT.Map.addExtentChangeHandler(onExtentChange);
                    FDOT.Map.addVisibleLayersChangeHandler(onLayersChange);
                    setFooterVisibility(FDOT.Map.getProperty("Footer.IsVisible"));
                }
            });

            $(window).resize(function () {
                if ($('#footer').is(":visible"))
                    $('#silverlightControlHost').height($(document).height() - $('#footer').height());
            });
        });

    </script>
</body>
</html>
