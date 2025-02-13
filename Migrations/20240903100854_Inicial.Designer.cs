﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebAppLibros.Models;

#nullable disable

namespace WebAppLibros.Migrations
{
    [DbContext(typeof(AppDBcontext))]
    [Migration("20240903100854_Inicial")]
    partial class Inicial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WebAppLibros.Models.Autor", b =>
                {
                    b.Property<int>("IdAutor")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdAutor"));

                    b.HasKey("IdAutor");

                    b.ToTable("Autores");
                });

            modelBuilder.Entity("WebAppLibros.Models.Categoria", b =>
                {
                    b.Property<int>("IdCategoria")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdCategoria"));

                    b.HasKey("IdCategoria");

                    b.ToTable("Categorias");
                });

            modelBuilder.Entity("WebAppLibros.Models.Estado", b =>
                {
                    b.Property<int>("IdEstado")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdEstado"));

                    b.Property<string>("Condición")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdEstado");

                    b.ToTable("Estados");
                });

            modelBuilder.Entity("WebAppLibros.Models.Idioma", b =>
                {
                    b.Property<int>("IdIdioma")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdIdioma"));

                    b.Property<string>("Tipo")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdIdioma");

                    b.ToTable("Idiomas");
                });

            modelBuilder.Entity("WebAppLibros.Models.Libro", b =>
                {
                    b.Property<int>("IdLibro")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdLibro"));

                    b.Property<double>("CalificacionPromedio")
                        .HasColumnType("float");

                    b.Property<int>("CantidadCopias")
                        .HasColumnType("int");

                    b.Property<int>("CantidadPags")
                        .HasColumnType("int");

                    b.Property<int>("IdEstado")
                        .HasColumnType("int");

                    b.Property<int>("IdIdioma")
                        .HasColumnType("int");

                    b.Property<string>("Titulo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdLibro");

                    b.HasIndex("IdEstado");

                    b.HasIndex("IdIdioma");

                    b.ToTable("Libros");
                });

            modelBuilder.Entity("WebAppLibros.Models.LibroAutor", b =>
                {
                    b.Property<int>("IdLibro")
                        .HasColumnType("int");

                    b.Property<int>("IdAutor")
                        .HasColumnType("int");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdLibro", "IdAutor");

                    b.HasIndex("IdAutor");

                    b.ToTable("LibrosAutores");
                });

            modelBuilder.Entity("WebAppLibros.Models.LibroCategoria", b =>
                {
                    b.Property<int>("IdLibro")
                        .HasColumnType("int");

                    b.Property<int>("IdCategoria")
                        .HasColumnType("int");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdLibro", "IdCategoria");

                    b.HasIndex("IdCategoria");

                    b.ToTable("LibrosCategorias");
                });

            modelBuilder.Entity("WebAppLibros.Models.UbicacionBiblioteca", b =>
                {
                    b.Property<int>("IdUbicacion")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdUbicacion"));

                    b.Property<string>("Estante")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("IdLibro")
                        .HasColumnType("int");

                    b.Property<string>("Seccion")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdUbicacion");

                    b.HasIndex("IdLibro")
                        .IsUnique();

                    b.ToTable("UbicacionesBiblioteca");
                });

            modelBuilder.Entity("WebAppLibros.Models.Libro", b =>
                {
                    b.HasOne("WebAppLibros.Models.Estado", "Estado")
                        .WithMany("Libros")
                        .HasForeignKey("IdEstado")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebAppLibros.Models.Idioma", "Idioma")
                        .WithMany("Libros")
                        .HasForeignKey("IdIdioma")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Estado");

                    b.Navigation("Idioma");
                });

            modelBuilder.Entity("WebAppLibros.Models.LibroAutor", b =>
                {
                    b.HasOne("WebAppLibros.Models.Autor", "Autor")
                        .WithMany("LibroAutores")
                        .HasForeignKey("IdAutor")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebAppLibros.Models.Libro", "Libro")
                        .WithMany("LibrosAutores")
                        .HasForeignKey("IdLibro")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Autor");

                    b.Navigation("Libro");
                });

            modelBuilder.Entity("WebAppLibros.Models.LibroCategoria", b =>
                {
                    b.HasOne("WebAppLibros.Models.Categoria", "Categoria")
                        .WithMany("LibrosCategorias")
                        .HasForeignKey("IdCategoria")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebAppLibros.Models.Libro", "Libro")
                        .WithMany("LibrosCategorias")
                        .HasForeignKey("IdLibro")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Categoria");

                    b.Navigation("Libro");
                });

            modelBuilder.Entity("WebAppLibros.Models.UbicacionBiblioteca", b =>
                {
                    b.HasOne("WebAppLibros.Models.Libro", "Libro")
                        .WithOne("UbicacionBiblioteca")
                        .HasForeignKey("WebAppLibros.Models.UbicacionBiblioteca", "IdLibro")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Libro");
                });

            modelBuilder.Entity("WebAppLibros.Models.Autor", b =>
                {
                    b.Navigation("LibroAutores");
                });

            modelBuilder.Entity("WebAppLibros.Models.Categoria", b =>
                {
                    b.Navigation("LibrosCategorias");
                });

            modelBuilder.Entity("WebAppLibros.Models.Estado", b =>
                {
                    b.Navigation("Libros");
                });

            modelBuilder.Entity("WebAppLibros.Models.Idioma", b =>
                {
                    b.Navigation("Libros");
                });

            modelBuilder.Entity("WebAppLibros.Models.Libro", b =>
                {
                    b.Navigation("LibrosAutores");

                    b.Navigation("LibrosCategorias");

                    b.Navigation("UbicacionBiblioteca");
                });
#pragma warning restore 612, 618
        }
    }
}
