using System.Collections.Generic;
using Commander.Models;

namespace Commander.Data
{
    public class MockCommanderRepo : ICommanderRepo
    {
        public void CreateCommand(Command cmd)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteCommand(Command cmd)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Command> GetAllCommands()
        {
            var commands = new List<Command>
            {
                new Command{Id=0, HowTo="First command", Line="Item1", Platform="Platform1"},
                new Command{Id=1, HowTo="Second command", Line="Item2", Platform="Platform2"},
                new Command{Id=2, HowTo="Third command", Line="Item3", Platform="Platform3"},
            };

            return commands;
        }

        public Command GetCommandById(int id) 
        {
            return new Command{Id=0, HowTo="First command", Line="Item1", Platform="Platform1"};
        }

        public bool SaveChanges()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateCommand(Command cmd)
        {
            throw new System.NotImplementedException();
        }
    }
}