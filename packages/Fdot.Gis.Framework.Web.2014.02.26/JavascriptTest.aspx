<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="JavascriptTest.aspx.cs"
    Inherits="FDOT.GIS.Web.TestEmbedded" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script type="text/javascript" src="Scripts/all.js"></script>
    <script type="text/javascript" src="Scripts/qunit-1.10.0.js"></script>
    <link rel="stylesheet" href="qunit-1.10.0.css" />
    <title>Javascript Test</title>
    <style type="text/css">
        html, body
        {
            height: 100%;
            overflow: auto;
        }
        
        body
        {
            padding: 0;
            margin: 0;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 100%;
        }
        
        #container
        {
            margin: 0 auto 0 auto;
        }
        
        #silverlightControlHost
        {
            float: left;
        }
        
        #testOptionsContainer
        {
            height: 394px;
            width: 45%;
            float: left;
            border: solid 1px #aaa;
            font-size: 0.8em;
            padding: 2px;
        }
        
        #toolbar
        {
            clear: both;
            background-color: #dedede;
            border-bottom: solid 1px #aaa;
            padding: 5px;
        }
        
        #toolbar ul
        {
            margin-left: 0;
            padding-left: 0;
            display: inline;
            border: none;
        }
        
        #toolbar ul li
        {
            margin-left: 0;
            padding-left: 2px;
            border: none;
            list-style: none;
            display: inline;
        }
        
        .testDataLayer
        {
            display: none;
        }
        
        .testDataText
        {
            width: 95%;
            height: 275px;
        }
        
        
        table
        {
            background-color: #337;
            font-size: 0.7em;
            border: solid 1px #444;
            -moz-border-radius: 10px;
            -webkit-border-radius: 10px;
            border-radius: 10px;
            margin: 10px;
        }
        
        table tr:last-child td:first-child
        {
            -moz-border-radius-bottomleft: 10px;
            -webkit-border-bottom-left-radius: 10px;
            border-bottom-left-radius: 10px;
        }
        table tr:last-child td:last-child
        {
            -moz-border-radius-bottomright: 10px;
            -webkit-border-bottom-right-radius: 10px;
            border-bottom-right-radius: 10px;
        }
        
        table thead
        {
            color: #FFF;
        }
        
        table tbody
        {
            background-color: #FFF;
        }
        
        table th
        {
            padding: 5px 10px;
        }
        
        table td
        {
            padding: 1px 7px;
        }
        
        table tr.odd
        {
            background-color: #DDD;
        }
        
        table caption
        {
            text-align: left;
            font-size: 1.2em;
            font-weight: bold;
            padding-left: 10px;
        }
    </style>
</head>
<body>
    <div id="container">
        <div id="silverlightControlHost">
        </div>
    </div>
    <div id="qunit">
    </div>
    <script type="text/javascript">
        $(function () {

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



            FDOT.Map.create
                ({
                    // need to override the defaults set in Map.js 
                    // when in standard mode, all are set to true there except showOverviewMap
                    // when in embedded mode, all are set to false
                    embedded: true,
                    showScalebar: true,
                    showNavigation: true,
                    containerSelector: '#silverlightControlHost',
                    id: 'slctl',
                    height: '400px',
                    width: '500px',
                    appId: urlParams.appId,
                    onReady: function () {
                        start();
                        test("ZoomToGeometry Test", function () {
                            FDOT.Map.executeCommand("zoomToGeometry", { geometry: { "XMin": -9383864.046945436, "XMax": -9380059.274520518,
                                "YMin": 3559129.4286369383, "YMax": 3561165.4348333674
                            }
                            }, function () {
                                ok(true, "Zoom to Geometry was successful.");
                            });
                        });

                        test("DrawCommand Test", function () {
                            FDOT.Map.executeCommand("draw", { "DrawingObjects": [{ "XMin": -9382121.04219691, "XMax": -9381883.243920352, "YMin": 3560013.6702045896, "YMax": 3560140.9205918666}]
                            }, function () {
                                ok(true, "DrawCommand was successful.");
                            });
                        });

                        test("ListLayersCommand Test", function () {
                            FDOT.Map.executeCommand('listlayers', null,
                    function (r) {
                        ok(r.Success, "Was Successful");

                        ok(r.Result.length > 0, "Count of Layers is greater then 0, Layers count: " + r.Result.length);
                    });
                        });
                    }
                });

        });

        test("Test 1", function () {
            ok(true, "Test");
        });
        

    </script>
</body>
</html>
