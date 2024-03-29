﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using model;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;


namespace view
{
    public partial class MainWindow : Form
    {

        private GMarkerGoogle marker;
        private GMarkerGoogle markerUser;
        private GMapOverlay markerOverlay;
        private DataTable dtCALI;
        private GestionArchivo modelo;

        public const double DEFAULT_LAT = 3.4113786;
        public const double DEFAULT_LONG = -76.5273724;
        private int filaSeleccionada;
        private double latitud;
        private double longitud;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void agregarUbicacionesCALI(bool accion)
        {
            dtCALI = new DataTable();
            dtCALI.Columns.Add(new DataColumn("Descripción ", typeof(string)));
            dtCALI.Columns.Add(new DataColumn("Latitud ", typeof(string)));
            dtCALI.Columns.Add(new DataColumn("Longitud ", typeof(string)));
            string[,] puntos = accion ? modelo.darCoordenadasCALI():modelo.darCoordenadasCALIEncontradas(cmBoxBarrio.Text,cmBoxServicio.Text);
            for (int i = 1; i < (puntos.Length/2)+1; i++)
            {
                dtCALI.Rows.Add("CALI "+i, puntos[i-1,0], puntos[i-1,1]);
            }
            dtGridCALI.DataSource = dtCALI;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            checkBoxMostrarInfo.Checked = true;
            modelo = new GestionArchivo();

            agregarUbicacionesCALI(true);

            //Agregando servicios
            string[] serv = modelo.darServicios();
            cmBoxServicio.Items.AddRange(serv);

            //Agregando barrios
            string[] barr = modelo.darBarrios();
            cmBoxBarrio.Items.AddRange(barr);

            //Desactivando columnas para que no aparezcan lat y ling
            dtGridCALI.Columns[1].Visible = dtGridCALI.Columns[2].Visible = false;

            latitud = 3.3419134;
            longitud = -76.5306936;
            gmap.DragButton = MouseButtons.Left;
            gmap.CanDragMap = true;
            gmap.MapProvider = GMapProviders.GoogleMap;
            gmap.Position = new PointLatLng(DEFAULT_LAT, DEFAULT_LONG);
            gmap.MinZoom = 12;
            gmap.MaxZoom = 18;
            gmap.Zoom = gmap.MinZoom;
            gmap.AutoScroll = true;
            //Marcador
            markerOverlay = new GMapOverlay("Marcador");
            marker = new GMarkerGoogle(new PointLatLng(latitud, longitud),GMarkerGoogleType.blue);
            markerUser = new GMarkerGoogle(new PointLatLng(DEFAULT_LAT, DEFAULT_LONG), GMarkerGoogleType.orange);
            markerOverlay.Markers.Add(marker); //Agregamos al mapa
            markerOverlay.Markers.Add(markerUser);
            cambiarCuadroTextoMarcadorUsuario(DEFAULT_LAT, DEFAULT_LONG);
            cambiarCuadroTextoMarcador(latitud, longitud);
            gmap.Overlays.Add(markerOverlay);
            actualizarGmap();
            
        }

        private void btnReiniciar_Click(object sender, EventArgs e)
        {
            cmBoxBarrio.Text = "";
            cmBoxServicio.Text = "";
            cambiarCuadroTextoMarcador(latitud, longitud);
            gmap.Position = new PointLatLng(DEFAULT_LAT, DEFAULT_LONG);
            gmap.Zoom = gmap.MinZoom;
            txtBoxLatitud.Text = Convert.ToString(latitud);
            txtBoxLongitud.Text = Convert.ToString(longitud);
            agregarUbicacionesCALI(true);
        }

        private void seleccionMarcador(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                filaSeleccionada = e.RowIndex;
                string[] rsp = new String[2];
                dtGridCALI.Rows[filaSeleccionada].Cells[2].Value.ToString().Split(',');
                rsp[0] = dtGridCALI.Rows[filaSeleccionada].Cells[1].Value.ToString().Replace('.', ',');
                rsp[1] = dtGridCALI.Rows[filaSeleccionada].Cells[2].Value.ToString().Replace('.', ',');
                double lat = Convert.ToDouble(rsp[0]);
                double lng = Convert.ToDouble(rsp[1]);
                cambiarCuadroTextoMarcador(lat, lng);
                actualizarLatyLng(lat,lng);
                gmap.Position = marker.Position;
                if (checkBoxMostrarInfo.Checked)
                {
                    PanelInformacion info = new PanelInformacion(this);
                    info.Show();
                }
            }
            catch (Exception)
            {

            }
            
        }

        private void gmap_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) //Para colocar marcador de la ubicación del usuario (naranja)
            {
                double lat = gmap.FromLocalToLatLng(e.X, e.Y).Lat;
                double lng = gmap.FromLocalToLatLng(e.X, e.Y).Lng;
                cambiarCuadroTextoMarcadorUsuario(lat, lng);
                actualizarLatyLng(lat, lng);
            }
        }

        private void cambiarCuadroTextoMarcador(double lat, double lng)
        {
            marker.Position = new PointLatLng(lat, lng);
            marker.ToolTipText = string.Format("Ubicación \nLatitud: {0:N7} \nLongitud: {1:N7}", lat, lng);
        }

        private void cambiarCuadroTextoMarcadorUsuario(double lat, double lng)
        {
            markerUser.Position = new PointLatLng(lat, lng);
            markerUser.ToolTipText = string.Format("Posición actual\n({0:N7},{1:N7})",lat,lng);
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            /** - NO SIRVE YA - GDirections direccion;
            PointLatLng inicio = new PointLatLng(markerUser.Position.Lat, markerUser.Position.Lng);
            PointLatLng final = new PointLatLng(marker.Position.Lat, marker.Position.Lng);
            var rutasDireccion = GMapProviders.GoogleMap.GetDirections(out direccion,inicio, final,false, false, false, false, false);
            GMapRoute rutaObtenida = new GMapRoute(direccion.Route,"Ruta ubicación");
            GMapOverlay capaRuta = new GMapOverlay("Capa de la ruta");
            capaRuta.Routes.Add(rutaObtenida);
            gmap.Overlays.Add(capaRuta);
            actualizarGmap();**/
            agregarUbicacionesCALI(false);
        }

        private void actualizarGmap()
        {
            gmap.Zoom += 1;
            gmap.Zoom -= 1;
        }

        private void actualizarLatyLng(double lat, double lng)
        {
            txtBoxLatitud.Text = lat.ToString();
            txtBoxLongitud.Text = lng.ToString();
        }

        public string[] darInformacionCALI()
        {
            string[] a = dtGridCALI.Rows[filaSeleccionada].Cells[0].Value.ToString().Split(' ');
            return modelo.darInformacionCALI(Convert.ToInt16(a[1]));
        }

    }
}
