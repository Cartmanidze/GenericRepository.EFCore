using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenericRepository.EFCore.Models
{
    /// <summary>
    /// Base table for EF Core contexts
    /// </summary>
    public abstract class BaseModel
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected BaseModel()
        {
            Oid = Guid.NewGuid();
        }

        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Column("Oid")]
        public Guid Oid { get; private set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Дата последнего сохранения
        /// </summary>
        public DateTime LastModifiedDate { get; set; }

        /// <summary>
        /// Кем создано
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Кем сохранено последний раз
        /// </summary>
        public Guid LastModifiedBy { get; set; }

        /// <summary>
        /// Флаг удаления
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}