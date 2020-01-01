using SummerBoot.Repository;
using System;
using System.Collections.Generic;

namespace Example.Models
{
    [Repository]
    public interface IPersonRepository:IRepository<Person>
    {
        [Select("select * from person where name=@name")]
        Person GetPersonsByName(string name);

        [Select("select * from person where birthDate>@birthDate")]
        Person GetPersonsByBirthDate(DateTime birthDate);

        [Select("select * from person where birthDate>@birthDate and name=@name")]
        Person GetPersonsByBirthDateAndName(CityDto dto,string name);

        [Select("select * from address where city=@city and id=@id")]
        List<Address> GetAddressByCityName(CityDto dto,string id);

        [Select("select count(id) from address where city=@city")]
        int GetAddressCountByCityName(string city);

        [Select("select birthDate from person where name=@name")]
        DateTime GetPersonBirthDayByName(string name);

    }
}