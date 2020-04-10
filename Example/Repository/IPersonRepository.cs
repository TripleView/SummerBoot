using SummerBoot.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Example.Models
{
    [Repository]
    //public interface IPersonRepository
    public interface IPersonRepository : IRepository<Person>
    {
        //[Select("select * from person where id=@id")]
        //Task<Person> GetPersonsByIdAsync(int id);

        [Update("update person set name=@name where id=@id" )]
        Task<int> UpdatePerson(string name,int id);

        [Select("select * from person")]
        Task<List<Person>> GetPersonsListByNameAsync();


        [Select("select * from person where id=@id")]
        Person GetPersonsById(int id);

        //[Select("select * from person")]
        //List<Person> GetPersonsListByName();
        [Select("select * from person")]
        Task<Page<Person>> GetPersonsListByPageAsync(IPageable pageable);

        [Select("select * from person where name=@name")]
        Person GetPersonsByName(string name);

        [Select("select count(*) from person")]
        int GetPersonTotalCount();

    }
}