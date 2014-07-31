﻿#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U4"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U8"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U1"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U2"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U4"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U8"/>
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U1)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U2)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U4)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U8)]
    public abstract class Convu : ILOp
    {
    }
}
