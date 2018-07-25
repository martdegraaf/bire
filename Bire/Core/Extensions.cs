﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bire
{
   public static class Extensions
   {

      public static void ResolvePlaceholders<T>(this IList<T> fields)
         where T : FieldValue
      {
         for (var i = 0; i < fields.Count; i++)
         {
            var f = fields[i];
            for (var j = i + 1; j < fields.Count; j++)
            {
               var fj = fields[j];
               if (fj.Value != null && f.Value != null)
               {
                  fj.Value = fj.Value.Replace(f.Field, f.Value);
               }
            }
         }
      }

      public static void Merge<T>(this IList<T> self, IList<FieldValue> fields, bool resetPrompt = false)
         where T : FieldValue
      {
         if(self == null)
         {
            throw new ArgumentNullException(nameof(fields));
         }

         if(fields == null || !fields.Any())
         {
            return;
         }

         foreach(var field in fields)
         {
            var match = self.Where(x => x.Field == field.Field).FirstOrDefault();
            if (match != null)
            {
               match.Value = field.Value;
               if (resetPrompt)
               {
                  match.CastTo<FieldValuePrompt>().Prompt = null;
               }
            }
            else
            {
               if (typeof(FieldValuePrompt).IsAssignableFrom(typeof(T)))
               {
                  self.Add(new FieldValuePrompt(field.Field, field.Value, null,null).CastTo<T>());
               }
               else
               {
                  self.Add(new FieldValue(field.Field, field.Value).CastTo<T>());
               }
            }
         }
      }

      public static T CastTo<T>(this object self)
      {
         return self == null ? default(T) : (T)self;
      }

      public static bool EqualsOrdinalIgnoreCase(this string self, string other)
      {
         if (self == null || other == null) return self == null && other == null;
         return string.Compare(self, other, StringComparison.OrdinalIgnoreCase) == 0;
      }

      internal static void MatchAllCases(this IList<FieldValuePrompt> self)
      {
         foreach (var f in self.ToList())
         {
            self.Add(new FieldValuePrompt($"#lower({f.Field})", f.Value?.ToLower(), null, null));
            self.Add(new FieldValuePrompt($"#upper({f.Field})", f.Value?.ToUpper(), null, null));
            self.Add(new FieldValuePrompt($"#dashed({f.Field})", f.Value?.Dashed(), null, null));
         }
      }

      public static string Dashed(this string self)
      {
         if (string.IsNullOrWhiteSpace(self)) return self;
         if (self.Contains("."))
         {
            return self.Trim().ToLower().Replace(".", "-");
         }
         return string
            .Concat(self.Trim()
            .Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + char.ToLower(x).ToString() : (i==0 ? char.ToLower(x).ToString() : x.ToString())));
      }

   }
}