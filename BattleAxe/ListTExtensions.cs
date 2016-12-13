using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleAxe {
    public static class ListTExtensions {
        public static void Remove<S, T>(this List<S> source, List<T> itemsToEvaluteForRemoval, Func<T, S, bool> matchForRemoval) {
            foreach (var item in itemsToEvaluteForRemoval) {
                var found = source.FirstOrDefault(s => matchForRemoval(item, s));
                if (found != null) {
                    source.Remove(found);
                }
            }
        }

        public static void Remove<T>(this List<T> source, Func<T, bool> removeIfMatch) {
            int pos = source.Count - 1;
            while (pos > -1) {
                var item = source[pos];
                if (removeIfMatch(item)) {
                    source.Remove(item);
                }
                pos--;
            }
        }

        public static void ActOn<S, T>(this List<S> source, List<T> itemsToEvaluate, Func<T, S, bool> whenMatched, Action<T, S> doThis) {
            foreach (var item in itemsToEvaluate) {
                var found = source.FirstOrDefault(s => whenMatched(item, s));
                if (found != null) {
                    doThis(item, found);
                }
            }
        }
    }
}
