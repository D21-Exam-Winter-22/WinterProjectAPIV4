using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterProjectAPIV4.DataTransferObjects;
using WinterProjectAPIV4.Models;

namespace WinterProjectAPIV4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserGroupController : ControllerBase
    {
        private readonly PaymentApidbContext context;
        private Dictionary<int, string> TokenDictionary;
        public UserGroupController(PaymentApidbContext context)
        {
            this.context = context;
            TokenDictionary = new Dictionary<int, string>();
        }

        [HttpGet("IsOnline")]
        public bool ApplicationIsOnline()
        {
            return true;
        }

        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<List<ShareUser>>> GetUsers()
        {
            var users = await context.ShareUsers.ToListAsync();
            if (users.Count == 0)
            {
                return NotFound(users);
            }
            return Ok(users);
        }

        [HttpGet("GetUserByID/{ID}")]
        public async Task<ActionResult<ShareUser>> GetUserOnID(int ID)
        {
            ShareUser SearchedUser = context.ShareUsers.Find(ID);
            if (SearchedUser == null)
            {
                return NotFound(SearchedUser);
            }
            return Ok(SearchedUser);
        }

        [HttpPost("CreateUser")]
        public async Task<ActionResult<List<ShareUser>>> CreateShareUser(CreateShareUserDto request)
        {
            
            //Check if the username already exists in the DB
            List<ShareUser> ExistingUsers = await context.ShareUsers.Where(User => User.UserName == request.UserName).ToListAsync();
            if (ExistingUsers.Count > 0)
            {
                return Conflict();
            }
            
            //Create the user to insert
            ShareUser UserToInsert = new ShareUser
            {
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password, 
                IsAdmin = request.IsAdmin
            };

            //Insert the user
            await context.ShareUsers.AddAsync(UserToInsert);
            await context.SaveChangesAsync();

            return await GetUsers();
        }

        //Update details of a user
        [HttpPut("UpdateUser")]
        public async Task<ActionResult<ShareUser>> UpdateUserDetails(ShareUser request)
        {
            ShareUser RecordToChange = context.ShareUsers.Find(request.UserId);
            if (RecordToChange != null)
            {
                RecordToChange.UserName = request.UserName;
                RecordToChange.PhoneNumber = request.PhoneNumber;
                RecordToChange.FirstName = request.FirstName;
                RecordToChange.LastName = request.LastName;
                RecordToChange.Email = request.Email;
                RecordToChange.Password = request.Password;
            }
            else
            {
                return NotFound(RecordToChange);
            }
            await context.SaveChangesAsync();
            return Ok(RecordToChange);
        }

        [HttpGet("GetDetailsOnUsername/{UserName}")]
        public async Task<ActionResult<CreateShareUserDto>> GetLogInDetails(string UserName)
        {
            List<ShareUser> UsersList = await context.ShareUsers.Where(User => User.UserName == UserName).ToListAsync();
            
            ShareUser SingleUser = UsersList.First();
            CreateShareUserDto User = null;
            
            if (UsersList.Count == 0)
            {
                return NotFound(User);
            }
            
            User = new CreateShareUserDto
            {
                UserName = SingleUser.UserName,
                PhoneNumber = SingleUser.PhoneNumber,
                FirstName = SingleUser.FirstName,
                LastName = SingleUser.LastName,
                Email = SingleUser.Email,
                IsAdmin = SingleUser.IsAdmin,
                Password = SingleUser.Password
            };
            
            return Ok(User);
        }

        [HttpGet("GetAllGroups")]
        public async Task<ActionResult<List<ShareGroup>>> GetAllGroups()
        {
            List<ShareGroup> AllGroupsList = await context.ShareGroups.ToListAsync();
            if (AllGroupsList == null)
            {
                return NotFound(AllGroupsList);
            }
            return Ok(AllGroupsList);
        }

        [HttpGet("GetGroupByGroupID/{GroupID}")]
        public async Task<ActionResult<List<UserGroup>>> GetAllGroupMembers(int GroupID)
        {
            List<UserGroup> GroupMembersList = await context.UserGroups
                .Include(UserGroup => UserGroup.User)
                .Include(Group => Group.Group)
                .Where(Group => Group.GroupId == GroupID).ToListAsync();
            
            
            return Ok(GroupMembersList);
        }

        [HttpGet("GetAllUserGroups")]
        public async Task<ActionResult<List<UserGroup>>> GetAllUserGroups()
        {
            List<UserGroup> GroupMembersList = await context.UserGroups
                .Include(UserGroup => UserGroup.User)
                .Include(Group => Group.Group).ToListAsync();

            return Ok(GroupMembersList);
        }

        [HttpPost("CreateGroup")]
        public async Task<ActionResult<List<ShareGroup>>> CreateGroup(CreateShareGroupDto request)
        {
            //TODO
            //Should we prevent groups with the same name?
            
            //Create a ShareGroup entry
            ShareGroup GroupToInsert = new ShareGroup
            {
                Name = request.Name,
                Description = request.Description,
                HasConcluded = request.HasConcluded
            };
            //Insert the Group
            await context.ShareGroups.AddAsync(GroupToInsert);
            await context.SaveChangesAsync();

            //Get the ID of inserted ShareGroup
            int NewlyCreatedID = GroupToInsert.GroupId;

            //Create a UserGroup entry
            UserGroup UserGroupToInsert = new UserGroup
            {
                UserId = request.UserID,
                GroupId = NewlyCreatedID,
                IsOwner = true,
            };

            await context.UserGroups.AddAsync(UserGroupToInsert);
            await context.SaveChangesAsync();

            return await GetAllGroups();
        }

        [HttpPost("JoinExistingGroup")]
        public async Task<ActionResult<UserGroup>> JoinGroup(JoinGroupDto request)
        {
            //Insert into UserGroup table
            ShareGroup ExistingGroup = context.ShareGroups.Find(request.GroupID);

            //Make sure you the user isn't already in the group
            var ExistingUserGroupQuery = from usergroup in context.UserGroups
                                         where usergroup.UserId == request.UserID && usergroup.GroupId == request.GroupID
                                         select new
                                         {
                                             usergroup.UserGroupId,
                                             usergroup.UserId,
                                             usergroup.GroupId
                                         };

            List<JoinGroupDto> ExistingUserGroupsList = new List<JoinGroupDto>();

            foreach (var record in ExistingUserGroupQuery)
            {
                ExistingUserGroupsList.Add(new JoinGroupDto
                {
                    UserID = record.UserId,
                    GroupID = record.GroupId
                });
            }


            if (ExistingUserGroupsList.Count > 0)
            {
                return BadRequest("Already in the group!");
            }
            if (ExistingGroup == null)
            {
                return NotFound("Group does not exist");
            }
            UserGroup UserGroupToInsert = new UserGroup
            {
                UserId = request.UserID,
                GroupId = request.GroupID,
                IsOwner = false
            };

            context.UserGroups.Add(UserGroupToInsert);
            await context.SaveChangesAsync();

            //Query the UserGroup for that GroupID

            List<UserGroup> UserGroups = await context.UserGroups
                .Where(UserGroup => UserGroup.GroupId == request.GroupID)
                .Include(UserGroup => UserGroup.User)
                .ToListAsync();
            return Ok(UserGroups);
        }

        [HttpPut("ConcludeGroup")]
        public async Task<ActionResult<ShareGroup>> ConcludeGroup(int GroupID)
        {
            var GroupToEnd = context.ShareGroups.Find(GroupID);
            if (GroupToEnd != null)
            {
                GroupToEnd.HasConcluded = true;
            }
            else
            {
                return NotFound("Group not found");
            }

            await context.SaveChangesAsync();
            ShareGroup ConcludedGroup = await context.ShareGroups.FindAsync(GroupID);
            return Ok(ConcludedGroup);
        }

        [HttpGet("GetAllExpenses")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> GetAllExpenses()
        {
            var query = from expense in context.Expenses
                        join usergroup in context.UserGroups on expense.UserGroupId equals usergroup.UserGroupId
                        join sharegroup in context.ShareGroups on usergroup.GroupId equals sharegroup.GroupId
                        join shareuser in context.ShareUsers on usergroup.UserId equals shareuser.UserId
                        select new
                        {
                            expense.ExpenseId,
                            expense.Amount,
                            ExpenseName = expense.Name,
                            ExpenseDescription = expense.Description,
                            usergroup.UserId,
                            usergroup.GroupId,
                            sharegroup.Name,
                            GroupDescription = sharegroup.Description,
                            shareuser.UserName,
                            shareuser.PhoneNumber,
                            shareuser.FirstName,
                            shareuser.LastName,
                            shareuser.Email
                        };

            List<GetAllExpensesDto> QueriedList = new List<GetAllExpensesDto>();
            foreach (var record in query)
            {
                QueriedList.Add(new GetAllExpensesDto
                {
                    ExpenseId = record.ExpenseId,
                    Amount = record.Amount,
                    UserId = record.UserId,
                    GroupId = record.GroupId,
                    GroupName = record.Name,
                    UserName = record.UserName,
                    PhoneNumber = record.PhoneNumber,
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    Email = record.Email,
                    ExpenseName = record.ExpenseName,
                    ExpenseDescription = record.ExpenseDescription,
                    GroupDescription = record.GroupDescription
                });
            }
            if (QueriedList.Count == 0)
            {
                return NotFound(QueriedList);
            }
            else
            {
                return Ok(QueriedList);
            }
            //return Ok(await context.Expenses.ToListAsync());
        }
        
        

        [HttpGet("GetAllExpensesOnGroupID/{GroupID}")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> getAllExpensesOnGroupID(int GroupID)
        {
            var query = from expense in context.Expenses
                        join usergroup in context.UserGroups on expense.UserGroupId equals usergroup.UserGroupId
                        join sharegroup in context.ShareGroups on usergroup.GroupId equals sharegroup.GroupId
                        join shareuser in context.ShareUsers on usergroup.UserId equals shareuser.UserId
                        where usergroup.GroupId == GroupID
                        select new
                        {
                            expense.ExpenseId,
                            expense.Amount,
                            ExpenseName = expense.Name,
                            ExpenseDescription = expense.Description,
                            usergroup.UserId,
                            usergroup.GroupId,
                            sharegroup.Name,
                            GroupDescription = sharegroup.Description,
                            shareuser.UserName,
                            shareuser.PhoneNumber,
                            shareuser.FirstName,
                            shareuser.LastName,
                            shareuser.Email,   
                           
                        };
            List<GetAllExpensesDto> QueriedList = new List<GetAllExpensesDto>();
            foreach (var record in query)
            {
                QueriedList.Add(new GetAllExpensesDto
                {
                    ExpenseId = record.ExpenseId,
                    Amount = record.Amount,
                    UserId = record.UserId,
                    GroupId = record.GroupId,
                    GroupName = record.Name,
                    UserName = record.UserName,
                    PhoneNumber = record.PhoneNumber,
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    Email = record.Email,
                    ExpenseName= record.ExpenseName,    
                    ExpenseDescription= record.ExpenseDescription,
                    GroupDescription= record.GroupDescription
                });
            }

            if (QueriedList.Count == 0)
            {
                return NotFound(QueriedList);
            }
            else
            {
                return Ok(QueriedList);
            }
        }

        [HttpPost("InsertExpenditure")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> InsertNewExpenditure(CreateExpenditureDto request)
        {
            //Get the UserGroupID
            var query = from usergroup in context.UserGroups
                        where usergroup.GroupId == request.GroupID &&
                              usergroup.UserId == request.UserID
                        select new
                        {
                            usergroup.UserGroupId
                        };


            int UserGroupID = -1;
            foreach (var usergroup in query)
            {
                UserGroupID = usergroup.UserGroupId;
            }
            //If the Usergroup for that expenditure doesn't exist
            if (UserGroupID == -1)
            {
                return NotFound(getAllExpensesOnGroupID(request.GroupID));
                //return BadRequest("Bad request");
            }

            //Insert into Expenses using the amount and UserGroupID

            Expense ExpenseToInsert = new Expense
            {
                UserGroupId = UserGroupID,
                Amount = request.Amount,
                Name = request.Name,    
                Description= request.Description
            };

            context.Expenses.Add(ExpenseToInsert);
            await context.SaveChangesAsync();

            return await getAllExpensesOnGroupID(request.GroupID);
        }

        [HttpDelete("DeleteExpenditure/{ExpenseID}")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> DeleteExpenditureOnID(int ExpenseID)
        {
            //Get the UserGroupID of this expenseID
            //Get the GroupID associate with the UserGroupID
            var GetUserGroupIDQuery = from expense in context.Expenses
                                      join usergroup in context.UserGroups on expense.UserGroupId equals usergroup.UserGroupId
                                      where expense.ExpenseId == ExpenseID
                                      select new
                                      {
                                          expense.UserGroupId,
                                          usergroup.UserId,
                                          usergroup.GroupId
                                      };
            int? UserGroupID = -1;
            int? UserID = -1;
            int? GroupID = -1;
            foreach (var record in GetUserGroupIDQuery)
            {
                UserGroupID = record.UserGroupId;
                UserID = record.UserId;
                GroupID = record.GroupId;
            }

            //Delete the expenseID from the table
            await context.Expenses.Where(x => x.ExpenseId == ExpenseID).ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            //Get all expenses for this GroupID
            return await getAllExpensesOnGroupID((int)GroupID);
        }

        [HttpGet("MoneyOwedByEveryoneInGroupID/{GroupID}")]
        public async Task<ActionResult<List<MoneyOwedByUserGroupDto>>> CalculateIndividualSharesInGroup(int GroupID)
        {
            //Get the group's name and description
            ShareGroup TheGroup = context.ShareGroups.Find(GroupID);
            
            //Calculating how much everyone paid from the group
            List<Expense> AllExpensesQuery = await context.Expenses
                .Include(Expense => Expense.UserGroup)
                .Include(Expense => Expense.UserGroup.User)
                .Include(Expense => Expense.UserGroup.Group)
                .Where(UserGroup => UserGroup.UserGroup.GroupId == GroupID)
                .ToListAsync();
            
            //Query to get the distinct userIDs
            List<UserGroup> DistinctUserIDsQuery =
                await context.UserGroups.Where(usergroup => usergroup.GroupId == GroupID).ToListAsync();

            // Group by UserID and sum the Amount
            //Get all the distinct UserIDs
           

            List<int> DistinctUserIDs = DistinctUserIDsQuery.Select(entry => (int)entry.UserId).ToList();

            //Sum up all their individual expenses and create a list of MoneyOwedByUserGroupDto Objects
            List<MoneyOwedByUserGroupDto> ListOfMoneyOwedByUsersGroup = new List<MoneyOwedByUserGroupDto>();

            foreach (var UserID in DistinctUserIDs)
            {
                double TotalAmountOwed = (double)AllExpensesQuery.Where(row => row.UserGroup.UserId == UserID)
                    .Sum(entries => entries.Amount);
                //Get the individual User's details
                List<ShareUser> CurrentUsers = await context.ShareUsers.Where(entry => entry.UserId == UserID).ToListAsync();
                ShareUser CurrentUser = CurrentUsers.First();
                
                //Construct the Dto to add to the list
                MoneyOwedByUserGroupDto MoneyOwedByCurrentUser = new MoneyOwedByUserGroupDto
                {
                    UserID = UserID,
                    GroupID = GroupID,
                    AmountPaidDuringGroup = TotalAmountOwed,
                    FirstName = CurrentUser.FirstName,
                    LastName = CurrentUser.LastName,
                    PhoneNumber = CurrentUser.PhoneNumber,
                    UserName = CurrentUser.UserName,
                    GroupName = TheGroup.Name,
                    GroupDescription = TheGroup.Description
                };
                ListOfMoneyOwedByUsersGroup.Add(MoneyOwedByCurrentUser);
            }

            //Calculate the group's total expenditure
            double GroupsTotalExpenditure = 0;
            double GroupSize = 0;

            foreach (var UsersExpenditure in ListOfMoneyOwedByUsersGroup)
            {
                GroupsTotalExpenditure += UsersExpenditure.AmountPaidDuringGroup;
                GroupSize++;
            }

            double AverageAmountPaidDuringGroup = GroupsTotalExpenditure / GroupSize;

            foreach (var UsersExpenditure in ListOfMoneyOwedByUsersGroup)
            {
                UsersExpenditure.FinalAmountOwed = AverageAmountPaidDuringGroup - UsersExpenditure.AmountPaidDuringGroup;
            }

            //Calculate AmountAlreadyPaid by every UserGroup
            List<InPayment> ListOfInPayments = await context.InPayments
                .Include(InPayment => InPayment.UserGroup)
                .Where(UserGroup => UserGroup.UserGroup.GroupId == GroupID)
                .ToListAsync();

            //Get the list of unique UserIDs in the ListOfInPayments
            List<int> ListOfDistinctInPaymentusers =
                ListOfInPayments.Select(entry => (int)entry.UserGroup.UserId).Distinct().ToList();

            //Calculate the total in payments for each user in the group
            //Assign it to the correct MoneyOwedByUserGroupDto for that user

            foreach (var UserID in ListOfDistinctInPaymentusers)
            {
                //Total for that user
                double TotalInPayment = (double)ListOfInPayments.Where(row => row.UserGroup.UserId == UserID)
                    .Sum(entries => entries.Amount);

                //Find the user in the ListOfMoneyOwedByUsersGroup and assign the AmountAlreadyPaid to TotalPayment
                MoneyOwedByUserGroupDto CurrentUser = ListOfMoneyOwedByUsersGroup.First(user => user.UserID == UserID);
                CurrentUser.AmountAlreadyPaid = TotalInPayment;
            }

            //Recalculate the FinalAmountOwed by subtracting the AmountAlreadyPaid from it

            foreach (var user in ListOfMoneyOwedByUsersGroup)
            {
                user.FinalAmountOwed -= user.AmountAlreadyPaid;
            }

            return Ok(ListOfMoneyOwedByUsersGroup);
        }


        [HttpPost("PayIntoGroupPool")]
        public async Task<ActionResult<List<MoneyOwedByUserGroupDto>>> PayIntoGroupPool(InPaymentDto request)
        {
            
            
            //Create an InPayment Object
            InPayment InPayment = new InPayment
            {
                UserGroupId = request.UserGroupId,
                Amount = request.Amount
            };
            //Insert it into the DB
            context.InPayments.Add(InPayment);
            await context.SaveChangesAsync();

            //Get GroupID from UserGroupID
            var GetGroupIDQuery = from usergroup in context.UserGroups
                                  where usergroup.UserGroupId == request.UserGroupId
                                  select new
                                  {
                                      usergroup.UserGroupId,
                                      usergroup.UserId,
                                      usergroup.GroupId
                                  };
            int GroupID = -1;
            foreach (var record in GetGroupIDQuery)
            {
                GroupID = (int)record.GroupId;
            }
            //Using the GroupID, query for all expenditures in the group

            return await CalculateIndividualSharesInGroup(GroupID);
        }

        [HttpGet("GetPersonalExpenses/{UserID}")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> GetAllPersonalExpenses(int UserID)
        {
            //Join Expenses to UserGroup to Group and User
            
            var ListOfExpensesQuery = from expense in context.Expenses
                join usergroup in context.UserGroups on expense.UserGroupId equals usergroup.UserGroupId
                join user in context.ShareUsers on usergroup.UserId equals user.UserId
                join sharegroup in context.ShareGroups on usergroup.GroupId equals sharegroup.GroupId
                where user.UserId == UserID
                    select new
                    {
                        expense.ExpenseId,
                        expense.Amount,
                        ExpenseName = expense.Name,
                        ExpenseDescription = expense.Description,
                        SelectedUserID = user.UserId,
                        user.UserName,
                        user.FirstName,
                        user.LastName,
                        user.PhoneNumber,
                        user.Email,
                        sharegroup.GroupId,
                        GroupName = sharegroup.Name,
                        GroupDescription = sharegroup.Description
                    };

            List<GetAllExpensesDto> ListOfPersonalExpenses = new List<GetAllExpensesDto>();

            foreach (var expense in ListOfExpensesQuery)
            {
                GetAllExpensesDto PersonalExpense = new GetAllExpensesDto
                {
                    ExpenseId = expense.ExpenseId,
                    Amount = expense.Amount,
                    UserId = expense.SelectedUserID,
                    GroupId = expense.GroupId,
                    GroupName = expense.GroupName,
                    UserName = expense.UserName,
                    PhoneNumber = expense.PhoneNumber,
                    FirstName = expense.FirstName,
                    LastName = expense.LastName,
                    Email = expense.Email,
                    ExpenseName = expense.ExpenseName,
                    ExpenseDescription = expense.ExpenseDescription,
                    GroupDescription = expense.GroupDescription
                };
                ListOfPersonalExpenses.Add(PersonalExpense);
            }

            if (ListOfExpensesQuery.Count() == 0)
            {
                return NotFound(ListOfPersonalExpenses);
            }

            return Ok(ListOfPersonalExpenses);
        }

        [HttpDelete("RemoveMemberFromGroup")]
        public async Task<ActionResult<List<UserGroup>>> RemoveMemberFromGroup(UserGroupDto request)
        {
            //TODO: IMPROVE THE SPEED OF THIS QUERY.
            //Delete related expenses and inpayments
            
            //Delete expenses
                //Get list of UserGroups
            List<UserGroup> usergroups = await context.UserGroups.Where(entry =>
                entry.UserId == request.UserID && entry.GroupId == request.GroupID).ToListAsync();
                //Using the usergroups, delete the expenses

                foreach (UserGroup usergroup in usergroups)
                {
                    await context.Expenses.Where(entry => entry.UserGroupId == usergroup.UserGroupId)
                        .ExecuteDeleteAsync();
                    await context.SaveChangesAsync();
                }
            
            
                //delete inpayments
            foreach (UserGroup usergroup in usergroups)
            {
                await context.InPayments.Where(entry => entry.UserGroupId == usergroup.UserGroupId)
                    .ExecuteDeleteAsync();
                await context.SaveChangesAsync();
            }

            //Have to delete all entries from UserGroup where the userID and GroupID match
            context.UserGroups.RemoveRange(context.UserGroups.Where(usergroup => usergroup.UserId == request.UserID && usergroup.GroupId == request.GroupID));
            await context.SaveChangesAsync();
            return await GetAllGroupMembers((int)request.GroupID);
        }

        [HttpPut("UpdateGroupDetails")]
        public async Task<ActionResult<List<UserGroup>>> UpdateGroupDetails(UpdateGroupDetailsDto request)
        {
            var query = from sharegroup in context.ShareGroups
                where sharegroup.GroupId == request.GroupID
                    select new
                    {
                        sharegroup.GroupId
                    };

            int rowCounter = 0;
            foreach (var row in query)
            {
                rowCounter++;
            }
            
            //Check if the record exists
            
            //Find the record
            ShareGroup RecordToChange = context.ShareGroups.Find(request.GroupID);
            
            RecordToChange.Name = request.NewGroupName;
            RecordToChange.Description = request.NewGroupDescription;

            //Save the changes
            await context.SaveChangesAsync();
            
            if (rowCounter == 0)
            {
                return NotFound(await GetAllGroupMembers((int)request.GroupID));
            }
            else
            {
                return Ok(await GetAllGroupMembers((int)request.GroupID));
            }
        }

        [HttpPut("EditAnExpense")]
        public async Task<ActionResult<List<GetAllExpensesDto>>> EditExpenditure(Expense request)
        {
            //Check if such an expenditure even exists
            var CheckIfExistsQuery = from expense in context.Expenses
                where expense.ExpenseId == request.ExpenseId
                select new
                {
                    expense.ExpenseId,
                    expense.UserGroupId
                };
            int counter = 0;
            int UserGroupID = -1;
            foreach (var entry in CheckIfExistsQuery)
            {
                counter++;
                UserGroupID = (int)entry.UserGroupId;
            }

            if (counter == 0)
            {
                return NotFound();
            }
            
            //Find the expenditure
            List<Expense> ListOfRecords = await context.Expenses.Where(expense => expense.ExpenseId == request.ExpenseId).ToListAsync();
            Expense RecordToUpdate = ListOfRecords.First();
            
            //Update them
            RecordToUpdate.Amount = request.Amount;
            RecordToUpdate.Name = request.Name;
            RecordToUpdate.Description = request.Description;
            await context.SaveChangesAsync();
            
            //Get the userID for the usergroupid

            UserGroup UserGroup = context.UserGroups.Find(UserGroupID);
            int UserID = (int)UserGroup.UserId;
            
            return await GetAllPersonalExpenses(UserID);
        }
        
        
        [HttpPut("EditInPayment")]
        public async void EditAnInPayment(InPaymentDto request)
        {
            //Get the particular inpayment entry
            InPayment RecordToUpdate = context.InPayments.Find(request.TransactionID);
            
            //Update the details
            {
                RecordToUpdate.UserGroupId = request.UserGroupId;
                RecordToUpdate.Amount = request.Amount;
            }
            await context.SaveChangesAsync();
        }
        
        
        [HttpGet("GetAllUserInPayments/{UserID}")]
        public async Task<ActionResult<List<UserInPaymentsDto>>> GetAllInPayments(int UserID)
        {
            var GetAllUserInPaymentsQuery = from inpayments in context.InPayments
                join usergroup in context.UserGroups on inpayments.UserGroupId equals usergroup.UserGroupId
                join shareUser in context.ShareUsers on usergroup.UserId equals shareUser.UserId
                where shareUser.UserId == UserID
                select new
                {
                    inpayments.TransactionId,
                    inpayments.Amount,
                    usergroup.UserGroupId,
                    GroupID = usergroup.GroupId,
                    shareUser.UserId,
                    shareUser.UserName,
                    shareUser.FirstName,
                    shareUser.LastName,
                    shareUser.PhoneNumber,
                    shareUser.Email
                };
            List<UserInPaymentsDto> ListOfUserInpayments = new List<UserInPaymentsDto>(); 
            foreach (var entry in GetAllUserInPaymentsQuery)
            {
                UserInPaymentsDto NewEntry = new UserInPaymentsDto
                {
                    TransactionID = entry.TransactionId,
                    Amount = entry.Amount,
                    UserGroupID = entry.UserGroupId,
                    GroupID = (int)entry.GroupID,
                    UserID = entry.UserId,
                    UserName = entry.UserName,
                    FirstName = entry.FirstName,
                    LastName = entry.LastName,
                    PhoneNumber = entry.PhoneNumber,
                    Email = entry.Email

                };
                ListOfUserInpayments.Add(NewEntry);
            }
            return Ok(ListOfUserInpayments);
        }
        
        //TODO
        [HttpGet("GetAllGroupInPayments/{GroupID}")]
        public async Task<ActionResult<List<InPaymentDto>>> GetAllGroupInPayments(int GroupID)
        {
            var GroupInPaymentsQuery = from inpayments in context.InPayments
                join usergroup in context.UserGroups on inpayments.UserGroupId equals usergroup.UserGroupId
                join user in context.ShareUsers on usergroup.UserId equals user.UserId
                join sharegroup in context.ShareGroups on usergroup.GroupId equals sharegroup.GroupId
                where usergroup.GroupId == GroupID
                select new
                {
                    inpayments.TransactionId,
                    inpayments.Amount,
                    usergroup.UserGroupId,
                    sharegroup.GroupId,
                    sharegroup.Name,
                    sharegroup.Description,
                    user.UserId,
                    user.UserName,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    user.Email
                };

            List<UserGroupPaymentDto> ListOfGroupInpayments = new List<UserGroupPaymentDto>();

            foreach (var entry in GroupInPaymentsQuery)
            {
                UserGroupPaymentDto NewEntry = new UserGroupPaymentDto
                {
                    TransactionID = entry.TransactionId,
                    PaidAmount = entry.Amount,
                    UserGroupID = entry.UserGroupId,
                    GroupID = entry.GroupId,
                    GroupName = entry.Name,
                    GroupDescription = entry.Description,
                    UserID = entry.UserId,
                    UserName = entry.UserName,
                    FirstName = entry.FirstName,
                    LastName = entry.LastName,
                    PhoneNumber = entry.PhoneNumber,
                    Email = entry.Email
                };
                ListOfGroupInpayments.Add(NewEntry);
            }
            return Ok(ListOfGroupInpayments);
        }
        
        
        [HttpGet("GetExpense/{ExpenseID}")]
        public async Task<ActionResult<Expense>> GetExpenseOnExpenseID(int ExpenseID)
        {
            return Ok(context.Expenses.Find(ExpenseID));
        }
        
        
        [HttpGet("GetInPayment/{TransactionID}")]
        public async Task<ActionResult<InPayment>> GetInPaymentOnTransactionID(int TransactionID)
        {
            return Ok(context.InPayments.Find(TransactionID));
        }
        
        
        [HttpDelete("DeleteUser/{UserID}")]
        public async Task<ActionResult<string>> DeleteUserOnUserID(int UserID)
        {
            //Get All the groups which this user is in (GroupIDs)
            List<UserGroup> ListOfParticipatingGroups = await context.UserGroups.Where(entry => entry.UserId == UserID).ToListAsync();
            //Then just user RemoveMemberFromGroup for everyone of those group IDs
            foreach (var usergroup in ListOfParticipatingGroups)
            {
                UserGroupDto UserGroupToRemove = new UserGroupDto
                {
                    UserID = usergroup.UserId,
                    GroupID = usergroup.GroupId
                };
                await RemoveMemberFromGroup(UserGroupToRemove);
            }
            //Then delete the user
            await context.ShareUsers.Where(entry => entry.UserId == UserID).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            return Ok("Deleted");

        }
        
        [HttpDelete("DeleteGroup/{GroupID}")]
        public async Task<ActionResult<string>> DeleteGroupOnGroupID(int GroupID)
        {
            //To delete a group, you need to first remove all members from that group
            
            //So find all the members in the group,
            List<UserGroup> ListOfParticipatingGroupMembers =
                await context.UserGroups.Where(entry => entry.GroupId == GroupID).ToListAsync();
            //Then user RemoveMemberFromGroup for every one of those members
            foreach (var usergroup in ListOfParticipatingGroupMembers)
            {
                UserGroupDto UserGroupToRemove = new UserGroupDto
                {
                    UserID = usergroup.UserId,
                    GroupID = usergroup.GroupId
                };
                await RemoveMemberFromGroup(UserGroupToRemove);
            }

            //Then delete the group
            await context.ShareGroups.Where(entry => entry.GroupId == GroupID).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            return Ok("Deleted");
        }
        
        [HttpDelete("DeleteInPayment/{TransactionID}")]
        public async Task<ActionResult<string>> DeleteInPaymentOnTransactionID(int TransactionID)
        {
            await context.InPayments.Where(entry => entry.TransactionId == TransactionID).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            return Ok("Deleted");
        }
        
        //TODO transfer ownership of group
        [HttpPut("TransferGroupOwnership")]
        public async Task<ActionResult<string>> TransferGroupOwnership(TransferGroupOwnershipDto request)
        {
            //Find the previous owner's UsergroupID
            List<UserGroup> GetCurrentOwneruserGroupQuery = await context.UserGroups
                .Where(entry => entry.UserId == request.PreviousOwnerUserID && entry.GroupId == request.GroupID)
                .ToListAsync();
            
            UserGroup CurrentOwnerUserGroup = GetCurrentOwneruserGroupQuery.First();
            //Set IsOwner to false
            CurrentOwnerUserGroup.IsOwner = false;
            //save changes
            await context.SaveChangesAsync();

            //Find the new Owner's Usergroup
            List<UserGroup> GetNewOwnerUserGroupQuery = await context.UserGroups
                .Where(entry => entry.UserId == request.NewOwnerUserID && entry.GroupId == request.GroupID)
                .ToListAsync();
            
            UserGroup NewOwnerUserGroup = GetNewOwnerUserGroupQuery.First();
            //Set IsOwner to true
            NewOwnerUserGroup.IsOwner = true;
            //save changes
            await context.SaveChangesAsync();

            return Ok("Transferred Ownership");
        }

        [HttpPost("Login")]
        public async Task<ActionResult<string>> LoginForToken(GetEncodingDto request)
        {
            string toEncode = request.Username + request.Password;
            string EncodedValue = Base64.Encode(toEncode);

            List<ShareUser> UsersList = await context.ShareUsers.Where(user => user.UserName == request.Username && user.Password == request.Password).ToListAsync();
            if (UsersList.Count == 0)
            {
                return NotFound("Invalid user details");
            }
            ShareUser user = UsersList.First();
            TokenDictionary.Add(user.UserId, EncodedValue);
            string token = TokenDictionary[user.UserId];
            return Ok(token);
        }

        
        
        //TODO  recover lost user account
        
    }
}
