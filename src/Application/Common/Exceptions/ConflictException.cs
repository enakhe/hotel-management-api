using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Application.Common.Exceptions;
public class ConflictException(string message) : Exception(message)
{
}
