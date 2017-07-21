using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Veterinarias
{
    public partial class Home : Form
    {
        private String stringConnection;
        public Home()
        {
            stringConnection = ConfigurationManager
                          .ConnectionStrings["cn"]
                          .ConnectionString;

            InitializeComponent();
        }

        private void Home_Load(object sender, EventArgs e)
        {
            cargarClientes();
            cargarMascotas();
            //guardarVenta();
            limpiarFormularioVentas();
            cargarGrillaVentas();
            cargarProductos();
            cargarGrillaDetalleVenta();
        }

        private void limpiarFormularioVentas()
        {
            txtfolio.Text = "";
            txtfolio.Enabled = true;
            dateTimePicker1.Text = "";
            //cbxcliente.SelectedIndex = 0;
            //cbxmascota.SelectedIndex = 0;
        }

        private void btnlimpiar_Click(object sender, EventArgs e)
        {
            limpiarFormularioVentas();
        }

        private void cargarClientes()
        {
            using (SqlConnection con = new SqlConnection(stringConnection))
            {
                try
                {
                    con.Open();
                    string query = "SELECT CliRut, CONCAT(CliNombre, ' ' , CliApellidoPaterno, ' ' , CliApellidoMaterno) AS AliasCliente FROM CLIENTE";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Cliente");

                    cbxcliente.DisplayMember = "AliasCliente";
                    cbxcliente.ValueMember = "CliRut";
                    cbxcliente.DataSource = dataSet.Tables["Cliente"];
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(
                    "Error al conectarse a la base de Datos : Al cargar Clientes" + ex,
                    ".: Sistema Veterinaria :.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                  );
                }
                finally
                {
                    con.Close();
                }
            }
        }

        private void cbxcliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarClientes();
        }

        private void cargarMascotas()
        {
            using (SqlConnection con = new SqlConnection(stringConnection))
            {
                int rutcliente = Convert.ToInt32(cbxcliente.SelectedValue.ToString());
                try
                {
                    con.Open();
                    string query = "SELECT MasCodigo, CONCAT(MasNombre, '-', MasEspecie) As Mascota FROM MASCOTA WHERE CliRut = '" +rutcliente+"'";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Mascota");

                    cbxmascota.DisplayMember = "Mascota";
                    cbxmascota.ValueMember = "MasCodigo";
                    cbxmascota.DataSource = dataSet.Tables["Mascota"];
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(
                    "Error al conectarse a la base de Datos : Al cargar Mascotas" + ex,
                    ".: Sistema Veterinaria :.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                   );
                }
                finally
                {
                    con.Close();
                }
            }
        }

        private void cbxmascota_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarMascotas();
        }

        private void guardarVenta()//Metodo btnguardar
        {
            if (txtfolio.Equals(""))
            {
                MessageBox.Show(
                "Faltan registros por ingresar en su formulario",
                ".: Sistema Veterinaria :.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
               );
            }
            else
            {
                using (SqlConnection con = new SqlConnection(stringConnection))
                {
                    con.Open();
                    try
                    {
                        SqlCommand cmd;

                        if (btnguardar.Text == "Guardar")
                        {
                            cmd = new SqlCommand("INSERT INTO VENTA(VenFolio, VenFecha, CliRut, MasCodigo) VALUES(@folio, @fecha, @clirut, @mascodigo)", con);
                        }
                        else
                        {
                            cmd = new SqlCommand("UPDATE VENTA SET VenFolio=@folio, VenFecha=@fecha, CliRut=@clirut, MasCodigo=@mascodigo", con);
                        }
                        cmd.Parameters.AddWithValue("@venfolio", txtfolio.Text);
                        cmd.Parameters.AddWithValue("@fecha", dateTimePicker1.Text);
                        cmd.Parameters.AddWithValue("@clirut", cbxcliente.SelectedValue);
                        cmd.Parameters.AddWithValue("@mascodigo", cbxmascota.SelectedValue);
                        cmd.ExecuteNonQuery();
                        limpiarFormularioVentas();
                    }
                    catch (SqlException eSql)
                    {
                        MessageBox.Show(
                         "Error al conectarse a la base de Datos : Al guardar Venta " + eSql,
                         ".: Sistema Veterinaria :.",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error
                         );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                         "Error al conectarse a la base de Datos : Al guardar Venta " + ex,
                         ".: Sistema Veterinaria :.",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error
                         );
                    }
                    finally
                    {
                        con.Close();
                        cargarGrillaVentas();
                    }
                }

            }
        }

        private void btnguardar_Click(object sender, EventArgs e)
        {
            guardarVenta();
        }

        private void cargarGrillaVentas()
        {
            using (SqlConnection con = new SqlConnection(stringConnection))
            {
                try
                {
                    string query = "SELECT VENTA.VenFolio, VenFecha, CONCAT(CliNombre, ' ', CliApellidoPaterno, ' ', CliApellidoMaterno) as Cliente, CONCAT(MasNombre, ' -- ', MasEspecie) as Mascota, isnull(SUM(DetPrecioTotal), 0) as total FROM VENTA " +
                                   "LEFT JOIN DETALLE_VENTA ON DETALLE_VENTA.VenFolio = VENTA.VenFolio " +
                                   "INNER JOIN CLIENTE ON CLIENTE.CliRut = VENTA.CliRut " +
                                   "INNER JOIN MASCOTA ON MASCOTA.CliRut = CLIENTE.CliRut AND MASCOTA.MasCodigo = VENTA.MasCodigo " +
                                   "GROUP BY VENTA.VenFolio, VenFecha, CONCAT(CliNombre, ' ', CliApellidoPaterno, ' ', CliApellidoMaterno) , CONCAT(MasNombre, ' -- ', MasEspecie)";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    con.Open();

                    DataSet ds = new DataSet();
                    da.Fill(ds, "GrillaVenta");
                    dataGridView1.Rows.Clear();//LIMPIA GRILLA, PARA QUE NO ACOPLE REGISTROS
                    foreach (DataRow row in ds.Tables["GrillaVenta"].Rows)
                    {
                        DataGridViewButtonCell btnver = new DataGridViewButtonCell();

                        int rows = dataGridView1.Rows.Add(
                                row[0],//Folio
                                row[1],//Fecha
                                row[2],//Cliente
                                row[3],//Mascota
                                row[4],//Venta
                                btnver//Boton Ver
                               );
                        dataGridView1.Rows[rows].Cells[5].Value = " ";
                    }
                }
                catch (SqlException es)
                {
                    MessageBox.Show(
                    "Error al conectarse a la base de Datos : Al listar Venta en Grilla" + es,
                    ".: Sistema Veterinaria :.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                   );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                    "Error al conectarse a la base de Datos : Al listar Venta en Grilla" + ex,
                    ".: Sistema Veterinaria :.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                   );
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            cargarGrillaVentas();
        }

        private void cargarProductos()
        {
            using (SqlConnection con = new SqlConnection(stringConnection))
            {
                try
                {
                    con.Open();
                    string query = "SELECT ProCodigo, ProNombre FROM PRODUCTO";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Producto");

                    cbxproducto.DisplayMember = "ProNombre";
                    cbxproducto.ValueMember = "ProCodigo";
                    cbxproducto.DataSource = dataSet.Tables["Producto"];
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(
                    "Error al conectarse a la base de Datos : Al cargar Productos" + ex,
                    ".: Sistema Veterinaria :.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                   );
                }
                finally
                {
                    con.Close();
                }
            }
        }

        private void cbxproducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarProductos();
        }

        private void GuardarDetalleVenta()
        {
            if (txtdetid.Equals("") || txtcantidad.Equals(""))
            {
                MessageBox.Show(
                "Faltan registros por ingresar en su formulario",
                ".: Sistema Veterinaria :.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
              );
            }
            else
            {
                using (SqlConnection con = new SqlConnection(stringConnection))
                {
                    //string ProCodigo = (cbxproducto.SelectedValue.ToString());
                    con.Open();
                    try
                    {
                        SqlCommand cmd;

                        if (btnguardar.Text == "Guardar")
                        {
                            cmd = new SqlCommand("INSERT INTO DETALLE_VENTA (DetId, ProCodigo, DetCantidad, DetPrecioUnitario, DetPrecioTotal ) VALUES ( @detid, @producto, @cantidad,(select ProPrecio from PRODUCTO where ProCodigo=" + cbxproducto.SelectedValue + "),((select ProPrecio from PRODUCTO where ProCodigo=" + cbxproducto.SelectedValue + ") * " + txtcantidad.Text + " ) )", con);
                        }
                        else
                        {
                            cmd = new SqlCommand("UPDATE DETALLE_VENTA SET DetId=@detid, ProCodigo=@producto, DetCantidad=@cantidad, DetPrecioUnitario=@preciounitario, DetPrecioTotal=@preciototal", con);
                        }
                        cmd.Parameters.AddWithValue("@detid", txtdetid.Text);
                        cmd.Parameters.AddWithValue("@cantidad", txtcantidad.Text);
                        cmd.Parameters.AddWithValue("@producto", cbxproducto.SelectedValue);
                        cmd.Parameters.AddWithValue("@preciounitario", null);
                        cmd.ExecuteNonQuery();
                        limpiarFormularioVentas();
                    }
                    catch (SqlException eSql)
                    {
                        MessageBox.Show(
                         "Error al conectarse a la base de Datos : Al guardar Detalle Venta " + eSql,
                         ".: Sistema Veterinaria :.",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error
                        );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                         "Error al conectarse a la base de Datos : Al guardar Detalle Venta " + ex,
                         ".: Sistema Veterinaria :.",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error
                        );
                    }
                    finally
                    {
                        con.Close();
                        cargarGrillaVentas();
                    }
                }

            }
        }

        private void btnagregar_Click(object sender, EventArgs e)
        {
            GuardarDetalleVenta();
        }

        private void cargarGrillaDetalleVenta()
        {
            using (SqlConnection con = new SqlConnection(stringConnection))
            {
                try
                {
                    string query = "select * from DETALLE_VENTA";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    con.Open();

                    DataSet ds = new DataSet();
                    da.Fill(ds, "detalle_venta");
                    dataGridView2.Rows.Clear();//LIMPIA GRILLA, PARA QUE NO ACOPLE REGISTROS
                    foreach (DataRow row in ds.Tables["detalle_venta"].Rows)
                    {
                        DataGridViewButtonCell btneditar = new DataGridViewButtonCell();
                        DataGridViewButtonCell btneliminar = new DataGridViewButtonCell();

                        btneditar.Value = " ";
                        btneliminar.UseColumnTextForButtonValue = true;

                        int fila = dataGridView2.Rows.Add(
                                row[0],//detid
                                row[1],//cantidad
                                row[2],//producto
                                row[3],//preciounitario
                                row[4],//total
                                btneditar,
                                btneliminar
                               );
                        dataGridView2.Rows[fila].Cells[5].Value = " ";
                        dataGridView2.Rows[fila].Cells[6].Value = " ";
                    }
                }
                catch (SqlException es)
                {
                    MessageBox.Show(
                    "Error al conectarse a la base de Datos : Al listar Producto en Grilla" + es,
                    ".: Sistema Veterinaria :.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                    "Error al conectarse a la base de Datos : Al listar Producto en Grilla" + ex,
                    ".: Sistema Veterinaria :.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                   );
                }
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {

                int DetId = Convert.ToInt32(dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString());

                #region esto es un ejemplo de dialogo
                /*          DialogResult respuesta =     MessageBox.Show(rut.ToString(), "Arriba del mensaje", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                          if (respuesta == DialogResult.Yes)
                          {
                              MessageBox.Show("click en si");

                          }
                          else if (respuesta == DialogResult.No)
                          {

                              MessageBox.Show("click en no");
                          }
                          else {

                              MessageBox.Show("En hora buena,el control no es un si no");
                          }*/
                #endregion

                if (dataGridView2.CurrentCell.ColumnIndex == 5)
                {

                    editarDetalleVenta(DetId);

                }
                else if (dataGridView2.CurrentCell.ColumnIndex == 6)
                {

                    eliminarDetalleVenta(DetId);
                }
            }
        }

        private void editarDetalleVenta(int detid)
        {
            using (SqlConnection cn = new SqlConnection(stringConnection))
            {
                try
                {
                    string query = "SELECT * FROM DETALLE_VENTA WHERE DetId = '" +detid+ "'";
                    SqlDataAdapter da = new SqlDataAdapter(query, cn);
                    cn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds, "GrillaDetalleVenta");
                    foreach (DataRow fila in ds.Tables["GrillaDetalleVenta"].Rows)
                    {
                        txtdetid.Text  = fila[0].ToString();
                        txtcantidad.Text = fila[1].ToString();
                        cbxproducto.SelectedValue = fila[2].ToString();
                        fila[3].ToString(); 
                        fila[4].ToString();

                        btnguardar.Text = "Editar";
                        btnguardar.Image = Properties.Resources.pencil_16;
                        txtdetid.Enabled = false;
                    }

                }
                catch (SqlException x)
                {
                    MessageBox.Show(
                    "Error al conectarse a la base de Datos : Al listar datos de Detalle Venta" + x.Message,
                    ".: Sistema Veterinaria :.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                   );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                    "Error al conectarse a la base de Datos : Al listar datos de Detalle Venta" + ex.Message,
                    ".: Sistema Veterinaria :.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                   );
                }
            }
        }

        private void eliminarDetalleVenta(int ProCodigo)
        {
            using (SqlConnection con = new SqlConnection(stringConnection))
            {
                con.Open();
                try
                {
                    DialogResult resultadoDialogo = MessageBox.Show("Esta seguro que quiere eliminar la Venta: " +detid, ".: Sistema Veterinaria :.", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (resultadoDialogo == DialogResult.Yes)
                    {
                        SqlCommand cmd = new SqlCommand("DELETE FROM DETALLE_VENTA WHERE DetId=@detid", con);
                        cmd.Parameters.AddWithValue("@detid", detid);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Producto eliminado exitosamente", ".. Sistema Veterinaria:.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Usuario cancelo acción de eliminación", ".. Sistema Veterinaria:.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                catch (SqlException eSql)
                {
                    MessageBox.Show(
                     "Error al conectarse a la base de Datos : Al eliminar Venta " + eSql,
                     ".: Sistema Veterinaria :.",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                     "Error al conectarse a la base de Datos : Al eliminar Venta " + ex,
                     ".: Sistema Veterinaria :.",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error
                     );
                }
                finally
                {
                    con.Close();
                    cargarGrillaDetalleVenta();
                }
            }
        }

        private void btnFinalizarPedido_Click(object sender, EventArgs e)
        {

        }

        private void cambiaSeleccionCliente()
        {

        }

        private void cargarMascotasSegunCliente(object sender, EventArgs e)
        {
            
        }

        private void crearcombobox(string combobox, string query, string table, string value)
        {

        }

}
}
