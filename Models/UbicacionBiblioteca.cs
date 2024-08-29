﻿namespace WebAppLibros.Models
{
    public class UbicacionBiblioteca
    {
        //La clave llamarla Id
        public int Id { get; set; }
        public string? Estante { get; set; }
        public string? Seccion { get; set; }

        //relación de uno a muchos
        public List<Libro>? Libros { get; set; }
    }


    //relación de uno a uno
    //public Libro? Libro { get; set; }
    //esto no funciona
}
