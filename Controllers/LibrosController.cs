﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpreadsheetLight;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppLibros.Models;

namespace WebAppLibros.Controllers
{
    public class LibrosController : Controller
    {
        private readonly AppDBcontext _context;
        private readonly IWebHostEnvironment _env;

        // inyección de dependencia SQL
        public LibrosController(AppDBcontext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        //public LibrosController()
        //{
        //    _context = new AppDBcontext();
        //}

        // GET: Libros
        public async Task<IActionResult> Index()
        {
            var appDBcontext = _context.Libros.Include(l => l.Estado).Include(l => l.Idioma).Include(l => l.Calificacion).Include(l => l.LibrosAutores).ThenInclude(la => la.Autor).Include(l => l.LibrosCategorias).ThenInclude(la => la.Categoria);
            //Agregando theninclude también se carga la información del
            //autor, no hay otra forma de hacerlo
            return View(await appDBcontext.ToListAsync());
        }

        // IMPORT
        public async Task<IActionResult> ImportarCsv()
        {
            var archivos = HttpContext.Request.Form.Files;
            if (archivos != null && archivos.Count > 0)
            {
                var archivo = archivos[0];
                if (archivo.Length > 0)
                {
                    var pathDestino = Path.Combine(_env.WebRootPath, "importaciones");
                    var archivoDestino = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(archivo.FileName);
                    var rutaDestino = Path.Combine(pathDestino, archivoDestino);

                    // Asegúrate de que la carpeta de destino exista
                    if (!Directory.Exists(pathDestino))
                    {
                        Directory.CreateDirectory(pathDestino);
                    }

                    using (var filestream = new FileStream(rutaDestino, FileMode.Create))
                    {
                        archivo.CopyTo(filestream);
                    }

                    try
                    {
                        using (var file = new FileStream(rutaDestino, FileMode.Open))
                        {
                            List<string> renglones = new List<string>();
                            List<Libro> LibrosArch = new List<Libro>();

                            using (StreamReader fileContent = new StreamReader(file, System.Text.Encoding.UTF8))
                            {
                                while (!fileContent.EndOfStream)
                                {
                                    renglones.Add(fileContent.ReadLine());
                                }
                            }
                            if (renglones.Count > 0)
                            {
                                foreach (var renglon in renglones)
                                {
                                    string[] data = renglon.Split(';');
                                    // Verificar si tiene exactamente 6 columnas
                                    if (data.Length == 6)
                                    {
                                        Libro libro = new Libro
                                        {
                                            Titulo = data[0].Trim(),
                                            CantidadCopias = int.Parse(data[1].Trim()),
                                            CantidadPags = int.Parse(data[2].Trim()),
                                            IdEstado = int.Parse(data[3].Trim()), // ID del estado
                                            IdIdioma = int.Parse(data[4].Trim()), // ID del idioma
                                            IdCalificacion = int.Parse(data[5].Trim()), // Calificación
                                        };
                                        LibrosArch.Add(libro);
                                    }
                                }

                                if (LibrosArch.Count > 0)
                                {
                                    _context.AddRange(LibrosArch);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Manejo de errores al leer el archivo o guardar en la base de datos
                        ModelState.AddModelError("", $"Error al procesar el archivo: {ex.Message}");

                        // Crea un modelo vacío o uno con datos relevantes para la vista "Error"
                        var errorModel = new ErrorViewModel
                        {
                            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                        };
                        return View("Error", errorModel);
                    }
                }
            }
            return RedirectToAction("Index", "Libros");
        }
        public async Task<IActionResult> ImportarExcel()
        {
            var archivos = HttpContext.Request.Form.Files;
            if (archivos != null && archivos.Count > 0)
            {
                var archivoExcel = archivos[0];
                if (archivoExcel.Length > 0)
                {
                    var rutaDestino = Path.Combine(_env.WebRootPath, "importaciones");
                    var extArch = Path.GetExtension(archivoExcel.FileName);
                    if (extArch == ".xlsx" || extArch == ".xls")
                    {
                        var archivoDestino = Guid.NewGuid().ToString().Replace("-", "") + extArch;
                        var rutaCompleta = Path.Combine(rutaDestino, archivoDestino);

                        using (var filestream = new FileStream(rutaCompleta, FileMode.Create))
                        {
                            archivoExcel.CopyTo(filestream);
                        }

                        SLDocument archXls = new SLDocument(rutaCompleta);
                        if (archXls != null)
                        {
                            List<Libro> ListaLibros = new List<Libro>();

                            int fila = 1;
                            while (!string.IsNullOrEmpty(archXls.GetCellValueAsString(fila, 1))) // Verifica si hay un título en la columna 1
                            {
                                try
                                {
                                    Libro libro = new Libro
                                    {
                                        Titulo = archXls.GetCellValueAsString(fila, 1), // Título del libro
                                        CantidadCopias = int.Parse(archXls.GetCellValueAsString(fila, 2)), // Cantidad de copias
                                        CantidadPags = int.Parse(archXls.GetCellValueAsString(fila, 3)), // Cantidad de páginas
                                        IdEstado = int.Parse(archXls.GetCellValueAsString(fila, 4)), // ID del estado
                                        IdIdioma = int.Parse(archXls.GetCellValueAsString(fila, 5)), // ID del idioma
                                        IdCalificacion = int.Parse(archXls.GetCellValueAsString(fila, 6)) // Calificación
                                    };

                                    ListaLibros.Add(libro);
                                }
                                catch (Exception ex)
                                {
                                    ViewData["Error"] = $"Error al procesar la fila {fila}: {ex.Message}";
                                    return View("Error");
                                }
                                fila++;
                            }

                            if (ListaLibros.Count > 0)
                            {
                                _context.AddRange(ListaLibros);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                ViewData["Error"] = "No se encontraron datos válidos en el archivo Excel.";
                            }
                        }
                    }
                }
            }
            return RedirectToAction("Index", "Libros");
        }



        // GET: Libros/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libro = await _context.Libros
                .Include(l => l.Estado)
                .Include(l => l.Idioma)
                .Include(l => l.Calificacion)
                .Include(l => l.LibrosAutores).ThenInclude(la => la.Autor)
                .Include(l => l.LibrosCategorias).ThenInclude(la => la.Categoria)
                .FirstOrDefaultAsync(l => l.IdLibro == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        // GET: Libros/Create
        public IActionResult Create()
        {
            ViewData["IdEstado"] = new SelectList(_context.Estados, "IdEstado", "Condición"); //id, info
            ViewData["IdIdioma"] = new SelectList(_context.Idiomas, "IdIdioma", "Tipo");
            ViewData["IdCalificacion"] = new SelectList(_context.Calificaciones, "IdCalificacion", "NumCalificacion");

            ViewBag.Autores = new MultiSelectList(_context.Autores, "IdAutor", "Nombre");
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "IdCategoria", "Tipo");

            return View();
        }

        // POST: Libros/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdLibro,Titulo,CantidadCopias,CantidadPags,IdEstado,IdIdioma,IdCalificacion,Foto")] Libro libro,
            List<int> autoresSeleccionados, List<int> categoriasSeleccionadas)
        {
            if (ModelState.IsValid)
            {
                // Manejo del archivo de foto
                var archivos = HttpContext.Request.Form.Files;
                if (archivos != null && archivos.Count > 0)
                {
                    var archivoFoto = archivos[0];
                    if (archivoFoto.Length > 0)
                    {
                        var rutaDestino = Path.Combine(_env.WebRootPath, "fotografias");
                        var extArch = Path.GetExtension(archivoFoto.FileName);
                        // Generar un nombre único para el archivo
                        var archivoDestino = Guid.NewGuid().ToString().Replace("-", "") + extArch;

                        // Guardar el archivo en memoria
                        using (var filestream = new FileStream(Path.Combine(rutaDestino, archivoDestino), FileMode.Create))
                        {
                            archivoFoto.CopyTo(filestream);
                            libro.Foto = archivoDestino; // Aquí asumes que la clase Libro tiene una propiedad Foto
                        }
                    }
                }

                // Guardar el libro en la base de datos
                _context.Add(libro);
                await _context.SaveChangesAsync();

                // Relacionar los autores seleccionados con el libro
                if (autoresSeleccionados != null && autoresSeleccionados.Count > 0)
                {
                    foreach (var idAutor in autoresSeleccionados)
                    {
                        var libroAutor = new LibroAutor
                        {
                            IdLibro = libro.IdLibro,
                            IdAutor = idAutor
                        };
                        _context.LibrosAutores.Add(libroAutor);
                    }
                    await _context.SaveChangesAsync();
                }

                // Relacionar las categorías seleccionadas con el libro
                if (categoriasSeleccionadas != null && categoriasSeleccionadas.Count > 0)
                {
                    foreach (var idCategoria in categoriasSeleccionadas)
                    {
                        var libroCategoria = new LibroCategoria
                        {
                            IdLibro = libro.IdLibro,
                            IdCategoria = idCategoria
                        };
                        _context.LibrosCategorias.Add(libroCategoria);
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            // En caso de error, cargar nuevamente los estados, idiomas y autores
            ViewData["IdEstado"] = new SelectList(_context.Estados, "IdEstado", "IdEstado", libro.IdEstado);
            ViewData["IdIdioma"] = new SelectList(_context.Idiomas, "IdIdioma", "IdIdioma", libro.IdIdioma);
            ViewData["IdCalificacion"] = new SelectList(_context.Calificaciones, "IdCalificacion", "NumCalificacion", libro.IdCalificacion);

            // Volver a cargar la lista de autores
            ViewBag.Autores = new MultiSelectList(_context.Autores, "IdAutor", "Nombre");
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "IdCategorias", "Tipo");
            return View(libro);
        }

        // GET: Libros/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener el libro con los relacionados
            var libro = await _context.Libros
                .Include(l => l.LibrosAutores)
                .ThenInclude(la => la.Autor)
                .Include(l => l.LibrosCategorias)
                .ThenInclude(la => la.Categoria)
                .FirstOrDefaultAsync(m => m.IdLibro == id);

            if (libro == null)
            {
                return NotFound();
            }

            // Obtener la lista de seleccionados (relacionados con el libro)
            var autoresSeleccionados = libro.LibrosAutores.Select(la => la.IdAutor).ToList();
            var categoriasSeleccionadas = libro.LibrosCategorias.Select(la => la.IdCategoria).ToList();

            // Comportamiento normal
            ViewData["IdEstado"] = new SelectList(_context.Estados, "IdEstado", "Condición", libro.IdEstado);
            ViewData["IdIdioma"] = new SelectList(_context.Idiomas, "IdIdioma", "Tipo", libro.IdIdioma);
            ViewData["IdCalificacion"] = new SelectList(_context.Calificaciones, "IdCalificacion", "NumCalificacion", libro.IdCalificacion);

            // Crear la lista de autores con los seleccionados marcados
            ViewBag.Autores = new MultiSelectList(_context.Autores, "IdAutor", "Nombre", autoresSeleccionados);

            // Crear la lista de categorías con los seleccionados marcados
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "IdCategoria", "Tipo", categoriasSeleccionadas);

            return View(libro);
        }


        // POST: Libros/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdLibro,Titulo,CantidadCopias,CantidadPags,IdEstado,IdIdioma,IdCalificacion,Foto")] Libro libro,
            List<int> autoresSeleccionados, List<int> categoriasSeleccionadas)
        {
            if (id != libro.IdLibro)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Manejo de archivos (fotos)
                    var archivos = HttpContext.Request.Form.Files;
                    if (archivos != null && archivos.Count > 0)
                    {
                        var archivoFoto = archivos[0];
                        if (archivoFoto.Length > 0)
                        {
                            var rutaDestino = Path.Combine(_env.WebRootPath, "fotografias");
                            var extArch = Path.GetExtension(archivoFoto.FileName);
                            var archivoDestino = Guid.NewGuid().ToString().Replace("-", "") + extArch;

                            // Crear el archivo en memoria
                            using (var filestream = new FileStream(Path.Combine(rutaDestino, archivoDestino), FileMode.Create))
                            {
                                archivoFoto.CopyTo(filestream);

                                // Si existe una foto anterior, se elimina
                                if (!string.IsNullOrEmpty(libro.Foto))
                                {
                                    string fotoAnterior = Path.Combine(rutaDestino, libro.Foto);
                                    if (System.IO.File.Exists(fotoAnterior))
                                    {
                                        System.IO.File.Delete(fotoAnterior);
                                    }
                                }

                                // Asignar la nueva foto
                                libro.Foto = archivoDestino;
                            }
                        }
                    }

                    // Actualizar libro en la base de datos
                    _context.Update(libro);
                    await _context.SaveChangesAsync();

                    // Eliminar relaciones antiguas en LibroAutor
                    var autoresAntiguos = _context.LibrosAutores.Where(la => la.IdLibro == libro.IdLibro);
                    _context.LibrosAutores.RemoveRange(autoresAntiguos);
                    await _context.SaveChangesAsync();

                    // Agregar nuevas relaciones en LibroAutor
                    if (autoresSeleccionados != null && autoresSeleccionados.Count > 0)
                    {
                        foreach (var IdAutor in autoresSeleccionados)
                        {
                            var libroAutor = new LibroAutor
                            {
                                IdLibro = libro.IdLibro,
                                IdAutor = IdAutor
                            };
                            _context.LibrosAutores.Add(libroAutor);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Eliminar relaciones antiguas en LibroCategoria
                    var categoriasAntiguas = _context.LibrosCategorias.Where(lc => lc.IdLibro == libro.IdLibro);
                    _context.LibrosCategorias.RemoveRange(categoriasAntiguas);
                    await _context.SaveChangesAsync();

                    // Agregar nuevas relaciones en LibroCategoria
                    if (categoriasSeleccionadas != null && categoriasSeleccionadas.Count > 0)
                    {
                        foreach (var IdCategoria in categoriasSeleccionadas)
                        {
                            var libroCategoria = new LibroCategoria
                            {
                                IdLibro = libro.IdLibro,
                                IdCategoria = IdCategoria
                            };
                            _context.LibrosCategorias.Add(libroCategoria);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LibroExists(libro.IdLibro))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // Si el modelo no es válido, vuelve a cargar las listas desplegables
            ViewData["IdEstado"] = new SelectList(_context.Estados, "IdEstado", "IdEstado", libro.IdEstado);
            ViewData["IdIdioma"] = new SelectList(_context.Idiomas, "IdIdioma", "IdIdioma", libro.IdIdioma);
            ViewData["IdCalificacion"] = new SelectList(_context.Calificaciones, "IdCalificacion", "IdCalificacion", libro.IdCalificacion);
            ViewBag.Autores = new MultiSelectList(_context.Autores, "IdAutor", "Nombre");
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "IdCategoria", "Tipo");
            return View(libro);
        }

        // GET: Libros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libro = await _context.Libros
                .Include(l => l.Estado)
                .Include(l => l.Idioma)
                .Include(l => l.Calificacion)
                .Include(l => l.LibrosAutores).ThenInclude(la => la.Autor)
                .Include(l => l.LibrosCategorias).ThenInclude(la => la.Categoria)
                .FirstOrDefaultAsync(m => m.IdLibro == id);
            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        // POST: Libros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libro = await _context.Libros.FindAsync(id);
            if (libro != null)
            {
                _context.Libros.Remove(libro);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LibroExists(int id)
        {
            return _context.Libros.Any(e => e.IdLibro == id);
        }
    }
}
