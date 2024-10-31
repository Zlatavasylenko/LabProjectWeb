using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LabProject.Models;

public partial class CastMember
{
    public int CastMemberId { get; set; }

    [Required(ErrorMessage = "Ім'я людини є обов'язковим")]
    [Display(Name = "Ім'я")]
    public string CastMemberFullName { get; set; }

    // Додаємо поле для зберігання шляху до фотографії
    [Display(Name = "Фото")]
    public string PhotoUrl { get; set; } // Це буде зберігати посилання на фото

    public virtual ICollection<MovieCast> MovieCasts { get; } = new List<MovieCast>();
}
