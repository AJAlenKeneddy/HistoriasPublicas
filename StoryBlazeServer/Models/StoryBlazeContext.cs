using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace StoryBlazeServer.Models
{
    public partial class StoryBlazeContext : DbContext
    {
        public StoryBlazeContext()
        {
        }

        public StoryBlazeContext(DbContextOptions<StoryBlazeContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Categorium> Categoria { get; set; } = null!;
        public virtual DbSet<Comentario> Comentarios { get; set; } = null!;
        public virtual DbSet<Fragmento> Fragmentos { get; set; } = null!;
        public virtual DbSet<Historia> Historias { get; set; } = null!;
        public virtual DbSet<Usuario> Usuarios { get; set; } = null!;
        public virtual DbSet<Voto> Votos { get; set; } = null!;
        public virtual DbSet<sp_ListarFragmentosPorHistoria>Sp_ListarFragmentosPorHistorias { get; set; } = null!;
        public virtual DbSet<sp_ListarHistorias> Sp_ListarHistorias { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categorium>(entity =>
            {
                entity.HasKey(e => e.CategoriaId)
                    .HasName("PK__Categori__F353C1E51A638054");

                entity.Property(e => e.Nombre).HasMaxLength(100);
            });

            modelBuilder.Entity<Comentario>(entity =>
            {
                entity.Property(e => e.ComentarioId).HasColumnName("ComentarioID");

                entity.Property(e => e.Comentario1)
                    .HasMaxLength(1000)
                    .HasColumnName("Comentario");

                entity.Property(e => e.FechaComentario)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FragmentoId).HasColumnName("FragmentoID");

                entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

                entity.HasOne(d => d.Fragmento)
                    .WithMany(p => p.Comentarios)
                    .HasForeignKey(d => d.FragmentoId)
                    .HasConstraintName("FK__Comentari__Fragm__38996AB5");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Comentarios)
                    .HasForeignKey(d => d.UsuarioId)
                    .HasConstraintName("FK__Comentari__Usuar__398D8EEE");
            });

            modelBuilder.Entity<Fragmento>(entity =>
            {
                entity.Property(e => e.FragmentoId).HasColumnName("FragmentoID");

                entity.Property(e => e.FechaCreacionFrag).HasColumnType("date");

                entity.Property(e => e.HistoriaId).HasColumnName("HistoriaID");

                entity.Property(e => e.TotalVotos).HasDefaultValueSql("((0))");

                entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

                entity.HasOne(d => d.Historia)
                    .WithMany(p => p.Fragmentos)
                    .HasForeignKey(d => d.HistoriaId)
                    .HasConstraintName("FK__Fragmento__Histo__2E1BDC42");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Fragmentos)
                    .HasForeignKey(d => d.UsuarioId)
                    .HasConstraintName("FK__Fragmento__Usuar__2F10007B");
            });

            modelBuilder.Entity<Historia>(entity =>
            {
                entity.Property(e => e.HistoriaId).HasColumnName("HistoriaID");

                entity.Property(e => e.Estado)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('En Curso')");

                entity.Property(e => e.FechaCreacion).HasColumnType("date");

                entity.Property(e => e.Resumen).HasMaxLength(1000);

                entity.Property(e => e.Titulo).HasMaxLength(200);

                entity.Property(e => e.UsuarioCreadorId).HasColumnName("UsuarioCreadorID");

                entity.HasOne(d => d.Categoria)
                    .WithMany(p => p.Historia)
                    .HasForeignKey(d => d.CategoriaId)
                    .HasConstraintName("FK_Historia_Categoria");

                entity.HasOne(d => d.UsuarioCreador)
                    .WithMany(p => p.Historia)
                    .HasForeignKey(d => d.UsuarioCreadorId)
                    .HasConstraintName("FK__Historias__Usuar__286302EC");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A1984046E71")
                    .IsUnique();

                entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

                entity.Property(e => e.CodigoRecuperacion).HasMaxLength(6);

                entity.Property(e => e.CodigoVerificacion).HasMaxLength(6);

                entity.Property(e => e.ContraseñaHash).HasMaxLength(256);

                entity.Property(e => e.Correo).HasMaxLength(100);

                entity.Property(e => e.FechaExpiracionCodigo).HasColumnType("datetime");

                entity.Property(e => e.FechaExpiracionCodigoRecuperacion).HasColumnType("datetime");

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.NombreUsuario).HasMaxLength(100);
            });

            modelBuilder.Entity<Voto>(entity =>
            {
                entity.Property(e => e.VotoId).HasColumnName("VotoID");

                entity.Property(e => e.FechaVoto)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FragmentoId).HasColumnName("FragmentoID");

                entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

                entity.Property(e => e.Voto1).HasColumnName("Voto");

                entity.HasOne(d => d.Fragmento)
                    .WithMany(p => p.Votos)
                    .HasForeignKey(d => d.FragmentoId)
                    .HasConstraintName("FK__Votos__Fragmento__32E0915F");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Votos)
                    .HasForeignKey(d => d.UsuarioId)
                    .HasConstraintName("FK__Votos__UsuarioID__33D4B598");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
