using Find_My_Boef.Controller;
using Find_My_Boef.DataContext;
using Find_My_Boef.Model;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ToastNotifications.Messages;

namespace Find_My_Boef.View
{
    public class Visualization
    {
        private static List<GMapPolygonColor> heatmaps = new();
        public enum MarkerImage
        {
            Substance,
            Officer,
            OfficerDisconnected,
            Assault,
            CameraWorking,
            CameraNotWorking,
            Accident,
            Unknown
        }
        public static void AddIconToMap(PointLatLng point, MarkerImage markerImage, IIcon owner, int id = 0)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                string uri = MarkerImageToUri(markerImage);
                double markerWidth = double.Parse(ConfigurationManager.AppSettings.Get("Icon_Width"));
                double markerHeight = double.Parse(ConfigurationManager.AppSettings.Get("Icon_Height"));
                BitmapImage bitmapImage = new(new Uri(uri));
                GMapMarker marker = new(point);

                System.Windows.Controls.Image image = new();
                image.Source = bitmapImage;
                image.Width = markerWidth;
                image.Height = markerHeight;
                image.Cursor = Cursors.Hand;
                marker.Shape = image;

                if (markerImage == MarkerImage.Officer || markerImage == MarkerImage.OfficerDisconnected)
                {
                    marker.Shape.MouseDown += new System.Windows.Input.MouseButtonEventHandler((s, e) => OfficerShape_MouseDown(s, e, id));
                }
                else if (markerImage == MarkerImage.CameraNotWorking || markerImage == MarkerImage.CameraWorking)
                {
                    marker.Shape.MouseDown += new System.Windows.Input.MouseButtonEventHandler((s, e) => Camera_MouseDown(s, e, id));
                }
                else
                {
                    marker.Shape.MouseDown += new System.Windows.Input.MouseButtonEventHandler((s, e) => Shape_MouseDown(s, e, id));
                }

                marker.Offset = new Point(-(markerWidth / 2), -markerHeight);
                marker.ZIndex = 99; // place markers on top of everything

                MainInstance.GetControl().Markers.Add(marker);

                owner.Marker = marker;
            });
        }

        private static void Camera_MouseDown(object s, MouseButtonEventArgs e, int id)
        {
            new CameraWindow(id).Show();
        }

        private static void Shape_MouseDown(object sender, MouseButtonEventArgs e, int id)
        {
            OffenseDataContext.CreateOffenseWindow(id, true);
        }

        private static void OfficerShape_MouseDown(object sender, MouseButtonEventArgs e, int id)
        {
            if (OfficerInformation.IsOfficerScreenOpen)
            {
                MapWindow.Notifier.ShowError("Er is al een werknemer-informatie scherm open.");
                return;
            }
            OfficerInformation.IsOfficerScreenOpen = true;
            OfficerInformation officerWindow = new(id);
            officerWindow.Show();
        }

        private static string MarkerImageToUri(MarkerImage markerImage)
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + markerImage switch
            {
                MarkerImage.Substance => "\\Images\\MarkerDelictSubstance.png",
                MarkerImage.Officer => "\\Images\\MarkerAgent.png",
                MarkerImage.OfficerDisconnected => "\\Images\\MarkerAgentVerbindingVerbroken.png",
                MarkerImage.Assault => "\\Images\\MarkerDelictAssult.png",
                MarkerImage.CameraWorking => "\\Images\\MarkerCameraWorking.png",
                MarkerImage.CameraNotWorking => "\\Images\\MarkerCameraNotWorking.png",
                MarkerImage.Unknown => "\\Images\\MarkerDelictUnknown.png",
                _ => "\\Images\\MarkerUnknown.png",
            };
        }

        public static void DrawPatrolRoute(PatrolRoute patrolRoute)
        {
            MainInstance.GetControl().Markers.Add(patrolRoute.Polygon);
        }

        public static void ClearPatrolRoute(PatrolRoute patrolRoute)
        {
            MainInstance.GetControl().Markers.Remove(patrolRoute.Polygon);
        }

        #region Heatmap
        /// <summary>
        /// Adds an overlay onto the existing map. 
        /// This overlay gets updated after adding / deleting an offense. 
        /// </summary>

        private static double _heatmapRatio = SettingsDataContext.GetDataContext().CurrentHeatMapSetting;

        public static void setHeatmapRatio(double ratio)
        {
            _heatmapRatio = ratio;
        }

        public static double GetHeatmapRatio()
        {
            return _heatmapRatio;
        }

        public static void AddHeatmap()
        {
            List<Offense> offenses = MainInstance.GetOffenses();
            double ratio = GetHeatmapRatio(); // The ratio of heatmap groups (range of lng/lat)     
            List<List<Offense>> offenseGroups = new List<List<Offense>>();

            foreach (Offense offense in offenses)
            {
                bool isCaptured = false; // See if the offense has already been added to an offensegroup (to prevent adding each offense * sum of count of list
                foreach (List<Offense> offenseGroup in offenseGroups)
                {
                    if (offenseGroup.Contains(offense))
                    {
                        isCaptured = true;
                    }
                }

                if (!isCaptured)
                {
                    List<Offense> offensesInRatio = new List<Offense>();
                    foreach (Offense o in offenses)
                    {
                        bool isCheckedIn = false;
                        foreach (List<Offense> offenseGroup in offenseGroups)
                        {
                            if (offenseGroup.Contains(o))
                            {
                                isCheckedIn = true;
                            }
                        }
                        if (o.Location.Lat < (offense.Location.Lat + ratio) && o.Location.Lat >
                                                                            (offense.Location.Lat - ratio)
                                                                            && (o.Location.Lng <
                                                                                (offense.Location.Lng + ratio) &&
                                                                                o.Location.Lng >
                                                                                (offense.Location.Lng - ratio))
                                                                            && !isCheckedIn)
                        {
                            offensesInRatio.Add(o);
                        }
                    }

                    if (offensesInRatio.Count > 1)
                    {
                        offenseGroups.Add(offensesInRatio);
                    }
                }
            }

            foreach (List<Offense> offenseGroup in offenseGroups)
            {
                List<PointLatLng> points = new List<PointLatLng>();

                foreach (Offense offense in offenseGroup)
                {
                    points.Add(new PointLatLng(offense.Location.Lat, offense.Location.Lng));
                }

                double maxLat = -90.0;
                double minLat = 90.0;
                double minLng = 180.0;
                double maxLng = -180.0;
                foreach (PointLatLng point in points)
                {
                    if (point.Lat > maxLat)
                    {
                        maxLat = point.Lat;
                    }
                    if (point.Lat < minLat)
                    {
                        minLat = point.Lat;
                    }
                    if (point.Lng > maxLng)
                    {
                        maxLng = point.Lng;
                    }
                    if (point.Lng < minLng)
                    {
                        minLng = point.Lng;
                    }
                }
                double avgLat = (maxLat + minLat) / 2;
                double avgLng = (maxLng + minLng) / 2;

                List<PointLatLng> square = new List<PointLatLng>();
                square.Add(new PointLatLng(minLat, minLng));
                square.Add(new PointLatLng(minLat, maxLng));
                square.Add(new PointLatLng(maxLat, maxLng));
                square.Add(new PointLatLng(maxLat, minLng));

                if (offenseGroup.Count > 1)
                {
                    CreateCircle(new PointLatLng(avgLat, avgLng), ratio, 20, offenseGroup.Count);
                }
            }
        }
        private static void CreateCircle(PointLatLng center, double radius, int segments, int severity)
        {
            if (severity > 5)
            {
                severity = 5;
            }
            severity -= 1;
            List<PointLatLng> pointList = new();

            double seg = Math.PI * 2 / segments;
            radius /= (3 - (.5 * severity));
            for (int i = 0; i < segments; i++)
            {
                double theta = seg * i;
                double a = center.Lat + Math.Cos(theta) * radius / 2;
                double b = center.Lng + Math.Sin(theta) * radius;

                PointLatLng point = new(a, b);

                pointList.Add(point);
            }
            GradientStopCollection gradientStopCollection = new();
            double offsetVar = 1 / (double)severity;
            for (int i = 1; i < severity + 1; i++)
            {
                gradientStopCollection.Add(new GradientStop()
                {
                    Color = Color.FromArgb(Convert.ToByte(75 + i * 20), Convert.ToByte(255), Convert.ToByte(165 - i * 33), 0),
                    Offset = 1 - (i - 1) * offsetVar
                });
            }
            GMapPolygonColor circle = new(pointList, Helper.FromArgb(0, 0, 0, 0), new RadialGradientBrush(gradientStopCollection));
            heatmaps.Add(circle);
            MainInstance.GetControl().Markers.Add(circle);
        }

        public static void ReloadHeatmaps()
        {
            foreach (GMapPolygonColor heatmap in heatmaps)
            {
                MainInstance.GetControl().Markers.Remove(heatmap);
            }

            heatmaps = new List<GMapPolygonColor>();
            AddHeatmap();
        }
        #endregion
    }
}
