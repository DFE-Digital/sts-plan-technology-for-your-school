#!/bin/bash
total_count=0
while read file; do
  count=$(xpath -q -e "//TestRun/ResultSummary/Counters/@total" "$file" 2>/dev/null | cut -d'"' -f2)
  echo "$file: $count tests executed"
  total_count=$((total_count + count))
done < <(find tests -name "*.trx")

if [ $total_count -le 0 ]; then
  echo "Error: No unit tests have been executed."
  exit 1
else
  echo "Total tests executed: $total_count"
fi
