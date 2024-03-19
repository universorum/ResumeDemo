using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ResumeDemo.Models;

public class Experience
{
    public          int Id       { get; private set; }
    public required int ResumeId { get; init; }


    [Display(Name = "公司")]
    public required string Company { get; set; }

    [Display(Name = "職位")]
    public required string Title { get; set; }

    [Display(Name = "開始於")]
    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
    public required DateOnly Start { get; set; }

    [Display(Name = "結束於")]
    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
    public required DateOnly? End { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; internal set; } = Guid.NewGuid();

    [JsonIgnore]
    public Resume? Resume { get; private set; }
}