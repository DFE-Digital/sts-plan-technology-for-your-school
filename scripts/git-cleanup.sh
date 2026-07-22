#!/usr/bin/env bash
#
# ============================================================================
# git-cleanup.sh
# ============================================================================
#
# Reports on the current state of branches in this repository, classifies
# them into cleanup tiers, and (optionally, with confirmation) performs the
# safe deletions.
#
# Usage:
#   ./git-cleanup.sh                # full report + tiers + cleanup prompt
#   ./git-cleanup.sh --report-only  # full report + tiers, no cleanup prompt
#   ./git-cleanup.sh --quick        # skip the raw report, just tiers + cleanup
#
# ----------------------------------------------------------------------------
# HOW THIS SCRIPT IS STRUCTURED (read this first)
# ----------------------------------------------------------------------------
# 1. Fetch  - sync your view of the remote so everything below is accurate.
# 2. Report - dump raw `git branch` output.
# 3. Classify - read that same data back in with `git` commands and sort
#    branches into three tiers using bash arrays.
# 4. Cleanup - print what tier 1 + tier 2 deletion would involve, ask for
#    explicit confirmation, then run it. Tier 3 is NEVER touched by this
#    script - it always needs a human to look at it.
#
# ============================================================================


# ----------------------------------------------------------------------------
# `set` changes bash's default behaviour for this script. By default, bash
# is very forgiving of errors (it just carries on), which is usually the
# wrong thing for a script that deletes branches. We tighten that up here:
#
#   -u  "unset variable" errors
#       Catches typos early.
#       If the script ever references a variable that was never assigned (e.g.
#       a typo like $BRANCHH instead of $BRANCH), bash exits immediately with
#       an error instead of silently treating it as an empty string.
#
#   -o pipefail
#       Normally, in a pipeline like `cmd1 | cmd2`, bash only looks at whether
#       the LAST command (cmd2) succeeded or failed, so cmd1's exit code is
#       discarded. pipefail makes the whole pipeline fail if ANY command in it
#       fails. This matters because we use pipelines a lot below, e.g.
#       `git branch | grep | sed`
#
# We deliberately do NOT use `-e` ("exit immediately on any error"), because
# later in the script we expect `git push` / `git branch -D` to sometimes
# fail (e.g. a protected branch) and we want to catch that failure
# ourselves and keep going, rather than have the whole script die.
# ----------------------------------------------------------------------------
set -uo pipefail

DEV_REF="origin/development"
MAIN_REF="origin/main"


# ----------------------------------------------------------------------------
# Argument parsing
# ----------------------------------------------------------------------------
# "$@" means "all the arguments passed to this script, each kept as a
# separate word" (as opposed to $* which would glob them into one string).
# This for-loop walks through each argument typed on the command line,
# e.g. running `./git-cleanup.sh --quick --report-only` would loop twice.
REPORT_ONLY=false
QUICK=false
for arg in "$@"; do
  # `case` is bash's version of a switch statement. Each pattern is
  # followed by `)`, the commands to run, then `;;` to end that branch.
  case "$arg" in
    --report-only) REPORT_ONLY=true ;;
    --quick) QUICK=true ;;
    # `*` is the fallback/wildcard match - anything not matched above.
    # `>&2` redirects the echo to stderr (file descriptor 2) rather than
    # stdout (file descriptor 1), which is the normal convention for error
    # messages - keeps them separate from the script's normal output.
    *) echo "Unknown option: $arg" >&2; exit 1 ;;
  esac
done


# ----------------------------------------------------------------------------
# Make sure we're inside a git repo, and move to its root
# ----------------------------------------------------------------------------
# `git rev-parse --show-toplevel` prints the absolute path to the top of
# the current repo (regardless of which subfolder you're sitting in when
# you run the script). We capture that output into REPO_ROOT using $(...)
# - this is "command substitution": run the command, take whatever it
# printed, and use it as a string.
#
# `2>/dev/null` throws away any error message rather than printing it
# (/dev/null is a special "black hole" file - anything written to it
# vanishes). We do this because if you're NOT in a git repo, this command
# fails and prints its own error, which we don't want cluttering the output
# - we print our own friendlier message instead.
#
# The `|| { ... }` part means "if the previous command failed (non-zero
# exit code), run this block instead." It's the bash equivalent of an
# if-the-command-failed check, written inline rather than as a full
# if/then/fi block.
REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null) || {
  echo "Not inside a git repository." >&2
  exit 1
}
cd "$REPO_ROOT"


# ----------------------------------------------------------------------------
# Small helper functions, purely for readable output formatting.
# In bash, a function is just `name() { commands }`. Once defined, you call
# it later like any other command, e.g. `section "My heading"`.
# ----------------------------------------------------------------------------

# Prints a horizontal rule of 70 dashes.
# {1..70} is "brace expansion" - bash expands it into the sequence
# 1 2 3 ... 70 before the command even runs. `printf '%.0s-' {1..70}`
# reuses the format string '%.0s-' once per item in that sequence, and
# '%.0s' means "consume an argument but print zero characters of it".
hr() { printf '%.0s-' {1..70}; echo; }

# Prints a blank line, a vertical rule, a heading, then another rule.
# "$1" is the function's first argument - same convention as $1 for
# script arguments, but scoped to whichever function you're inside.
section() { echo; hr; echo "$1"; hr; }


# ============================================================================
# 1. FETCH
# ============================================================================
# `git fetch` downloads new commits/branches from the remote ("origin")
# without touching working files or local branches.
#
#   --all         Fetch from every configured remote, not just "origin"
#   --prune       If a branch has been deleted on the remote since you last
#                 fetched, delete your local record of it too. This is what
#                 causes branches to show as "[origin/branch: gone]" in
#                 `git branch -vv`; without --prune, git would keep a stale
#                 reference forever after the remote branch is gone.
#   --prune-tags  Same idea, but for tags instead of branches
#
# Running this first means every command below is working from an
# up-to-date picture of what's actually on the remote.
echo "Fetching all branches and tags from origin..."
git fetch --all --prune --prune-tags


# ============================================================================
# 2. RAW REPORT  (skipped entirely if you pass --quick)
# ============================================================================
if [ "$QUICK" = false ]; then

  # `git branch -vv` ("verbose verbose") lists LOCAL branches, showing for
  # each one: the branch name, its latest commit hash, which remote branch
  # it's tracking (in square brackets, plus "gone" if that remote branch
  # no longer exists), and the latest commit message.
  section "Local branches"
  git branch -vv

  # `--merged <ref>` filters the branch list down to only branches whose
  # entire commit history is already reachable from <ref>. In plain terms:
  # "this branch has nothing in it that development/main doesn't already
  # have" - which is exactly what makes a branch safe to delete.
  section "Local branches merged into development"
  git branch -vv --merged "$DEV_REF"

  section "Local branches merged into main"
  git branch -vv --merged "$MAIN_REF"

  # `-r` switches from local branches to REMOTE-TRACKING branches - i.e.
  # your local copy of what exists on origin (things like origin/main,
  # origin/some-feature). These aren't branches you can commit to directly;
  # they're git's local bookkeeping of what it saw on the server as of your
  # last fetch.
  section "Remote branches"
  git branch -rvv

  section "Remote branches merged into development"
  git branch -rvv --merged "$DEV_REF"

  section "Remote branches merged into main"
  git branch -rvv --merged "$MAIN_REF"

  # `git for-each-ref` is a lower-level, more flexible way to list refs
  # (branches, tags, etc.) than `git branch`. The advantage here is
  # `--format`, which lets us pick exactly which fields to print and in
  # what order - things like commit date and author name, which plain
  # `git branch` can't show you at all.
  #
  #   refs/remotes/origin    Restrict to remote-tracking branches under origin
  #   --sort=-committerdate  Sort by commit date, newest first (the leading
  #                          "-" reverses the default ascending sort)
  #   %(committerdate:short) / %(authorname) / %(refname:short)
  #                          Placeholders git fills in per branch
  #   %09                    A placeholder that means "insert a tab
  #                          character here" - using this instead of typing
  #                          a literal tab keeps the --format string
  #                          readable and avoids any risk of an editor
  #                          silently converting tabs to spaces
  #
  # As with the unmerged-branches table further down, we build this as a
  # header row plus data rows separated by tabs, then hand the whole thing
  # to `column -t` to line the columns up neatly regardless of how long
  # each branch name or author name is.
  #
  # The `grep -v -E 'origin/HEAD$'` removes the "origin/HEAD" line, which
  # isn't a real branch - it's just a pointer to whichever branch origin
  # considers its default (typically main). `-v` means "invert the match"
  # (show lines that DON'T match), `-E` enables "extended regex" syntax
  # (needed for the `$` end-of-line anchor).
  section "All remote branches, sorted by commit date"
  {
    printf "Date\tAuthor\tBranch\n"
    git for-each-ref --sort=-committerdate refs/remotes/origin \
      --format='%(committerdate:short)%09%(authorname)%09%(refname:short)' \
      | grep -v -E 'origin/HEAD$'
  } | column -t -s $'\t'

  # This block builds a table showing which branches are genuinely unmerged,
  # and by how much
  section "Remote branches NOT merged into development or main"
  echo "(Sorted: Ahead by descending - branches most likely to have real, unmerged work are at the top)"
  {
    # A header row for the table.
    printf "Branch\tBehind by\tAhead by\n"

    # `git branch -r` lists all remote-tracking branches (plain names,
    # no verbose info this time). We filter out origin/HEAD, and also
    # filter out main/development themselves since comparing them to
    # development doesn't tell us anything useful.
    #
    # `$(...)` here runs that filtered branch list and feeds each
    # resulting word into the `for` loop as $branch, one at a time.
    for branch in $(git branch -r | grep -v HEAD | grep -vE '/(main|development)$'); do

      # `git rev-list --left-right --count A...B` compares two branches
      # and counts commits that are unique to each side of a "triple-dot"
      # range. In plain terms: how many commits are in development but
      # not in this branch (behind), and how many are in this branch but
      # not in development (ahead). `--left-right` tags each commit with
      # which side it came from so `--count` can total them separately -
      # without it you'd just get one combined number.
      #
      # `2>/dev/null` again swallows any error output (e.g. if a branch
      # somehow vanished between the earlier `git branch -r` and this
      # line running).
      counts=$(git rev-list --left-right --count "$DEV_REF...$branch" 2>/dev/null)

      # `git rev-list --left-right --count` prints its two numbers
      # separated by a tab, like "212  2". awk '{print $1}' and
      # '{print $2}' just pull out the first and second fields.
      behind=$(echo "$counts" | awk '{print $1}')
      ahead=$(echo "$counts" | awk '{print $2}')

      # "${branch#origin/}" strips the literal prefix "origin/" off the
      # front of $branch (the "#" means "remove the shortest match of
      # this pattern from the start of the string"). So
      # "origin/feature/x" becomes just "feature/x" for display.
      #
      # "${behind:-?}" means "use $behind, but if it's empty/unset, use
      # '?' instead" - a safety net in case the rev-list command above
      # failed and counts came back blank.
      printf "%s\t%s\t%s\n" "${branch#origin/}" "${behind:-?}" "${ahead:-?}"
    done
  # Finally, feed that into a pipeline:
  #   (read -r header; echo "$header"; sort -t$'\t' -k3 -rn)
  #     runs a subshell: reads the FIRST line ("Branch\tBehind by\tAhead by\n")
  #     into a variable and immediately echoes it back out untouched, so the
  #     header always stays pinned at the top. Every remaining line then
  #     falls through to `sort`, which sorts by the 3rd tab-separated
  #     field (-k3, the "Ahead by" column), numerically (-n) and in
  #     reverse/descending order (-r), using tab as the field separator
  #     (-t$'\t'). The $'...' syntax lets bash interpret \t as an actual
  #     tab character rather than the two literal characters "\" and "t".
  #   column -t -s $'\t'
  #     reformats tab-separated input into a neatly aligned table,
  #     padding each column to the width of its longest entry.
  } | (read -r header; echo "$header"; sort -t$'\t' -k3 -rn) | column -t -s $'\t'
fi


# ============================================================================
# 3. CLASSIFY
# ============================================================================
# This is the part that turns raw git output into three tiers:
# - Tier 1: Safe to delete, no discussion needed
# - Tier 2: Local-only cleanup (the [gone] local branches)
# - Tier 3: Needs a human look (unmerged, or only merged into one branch)
#
# Rather than eyeballing the report above and deciding by hand, we re-run the
# same underlying git commands and capture their output into bash arrays,
# then use set logic (union / difference) to sort branches.
section "Classification"

# `mapfile -t ARRAYNAME < <(command)` is a common bash pattern meaning
# "run this command, and load its output into an array, one array element
# per line of output." The `-t` strips the trailing newline from each line
# as without it, each element would have an invisible \n stuck on the end.
# The `< <(...)` is "process substitution" which lets a command's output be
# read as if it were a file, which is what mapfile needs on its input side.
#
# Here we get every remote branch merged into development, then run it
# through a small pipeline to clean it up:
#   sed 's/^[* ]*//'  Strip any leading "* " or spaces as git prefixes the
#                     currently checked-out branch with "* " in some listings.
#   grep -v '\->'     Remove the "origin/HEAD -> origin/main" line (the arrow
#                     only appears on that one line)
#   grep -vE '/(HEAD|main|development)$'
#                     Remove origin/HEAD, origin/main and origin/development
#                     because we only want OTHER branches in this list
mapfile -t merged_dev < <(git branch -r --merged "$DEV_REF" \
  | sed 's/^[* ]*//' | grep -v '\->' | grep -vE '/(HEAD|main|development)$')

# Same idea, but for branches merged into main.
mapfile -t merged_main < <(git branch -r --merged "$MAIN_REF" \
  | sed 's/^[* ]*//' | grep -v '\->' | grep -vE '/(HEAD|main|development)$')

# TIER 1 = merged into development OR main. A branch only needs to have
# reached one protected branch to be considered safe - once its commits
# exist on a protected branch, deleting the branch pointer itself loses
# nothing, since the code lives on regardless.
#
# This is a set UNION (merged_dev combined with merged_main), rather than
# an intersection, so a branch merged into only one of the two still
# qualifies. The one wrinkle with a union is that a branch merged into
# BOTH would otherwise be listed twice - `declare -A tier1_seen` is used
# purely as a lookup table to catch and skip duplicates as we build the
# list, the same "have we seen this key before" trick used elsewhere in
# this script for fast membership checks.
declare -A tier1_seen
TIER1=()
for b in "${merged_dev[@]}" "${merged_main[@]}"; do
  # "${tier1_seen[$b]:-}" looks up $b in the tier1_seen array; if it's
  # not there, ":-" supplies an empty string instead of erroring (remember
  # `set -u` above - without this fallback, looking up a missing key
  # would kill the script).
  if [[ -z "${tier1_seen[$b]:-}" ]]; then
    tier1_seen["$b"]=1
    TIER1+=("$b")
  fi
done

# TIER 2 = local branches whose remote counterpart is gone.
# We reuse `git branch -vv` from the report above, but this time filter
# for the literal text ": gone]" that git prints when --prune (in our
# fetch step) discovered the remote branch no longer exists.
#   sed 's/^[* ]*//'   Strip the "* " marker again
#   awk '{print $1}'   The branch name is always the first field/column
mapfile -t TIER2 < <(git branch -vv | grep ': gone]' | sed 's/^[* ]*//' | awk '{print $1}')

# TIER 3 = every other remote branch, i.e. everything NOT in Tier 1.
# First, get the full list of remote branches (minus HEAD/main/development).
mapfile -t all_remote < <(git branch -r | grep -v HEAD | grep -v '\->' | grep -vE '/(main|development)$' | sed 's/^ *//')

# Build a lookup set for Tier 1, same trick as before, so we can quickly
# test "is this branch already accounted for in Tier 1?"
declare -A in_tier1
for b in "${TIER1[@]}"; do in_tier1["$b"]=1; done

# Anything in all_remote that ISN'T in the Tier 1 set falls into Tier 3 -
# this is a set difference (all_remote minus TIER1).
TIER3=()
for b in "${all_remote[@]}"; do
  if [[ -z "${in_tier1[$b]:-}" ]]; then
    # -z is the opposite of -n: true when the string IS empty, i.e. the
    # lookup found nothing, i.e. this branch is not in Tier 1.
    TIER3+=("$b")
  fi
done

# Print the three tiers. ${#ARRAY[@]} gives the number of elements in an
# array - used here just to detect the "empty tier" case so we print
# "(none)" instead of nothing at all.
echo "Tier 1 - Safe to delete on remote (merged into development or main):"
if [ ${#TIER1[@]} -eq 0 ]; then echo "  (none)"; else printf '  %s\n' "${TIER1[@]}"; fi

echo
echo "Tier 2 - Safe to delete locally (remote already gone):"
if [ ${#TIER2[@]} -eq 0 ]; then echo "  (none)"; else printf '  %s\n' "${TIER2[@]}"; fi

echo
echo "Tier 3 - Needs a human look (unmerged, or only merged into one branch):"
if [ ${#TIER3[@]} -eq 0 ]; then echo "  (none)"; else printf '  %s\n' "${TIER3[@]}"; fi

# If you passed --report-only, stop here - no cleanup, no confirmation
# prompt, nothing destructive happens at all.
if [ "$REPORT_ONLY" = true ]; then
  echo
  echo "--report-only set: stopping before cleanup."
  exit 0
fi


# ============================================================================
# 4. CLEANUP  (Tier 1 + Tier 2 only - Tier 3 is NEVER touched automatically)
# ============================================================================
section "Automated cleanup"

if [ ${#TIER1[@]} -eq 0 ] && [ ${#TIER2[@]} -eq 0 ]; then
  echo "Nothing safe to clean up automatically."
  exit 0
fi

# Show every command this script is about to run, BEFORE running any of
# them, so you can read exactly what will happen and cancel if something
# looks wrong.
echo "This will run the following DESTRUCTIVE commands:"
for b in "${TIER1[@]}"; do echo "  git push origin --delete ${b#origin/}"; done
for b in "${TIER2[@]}"; do echo "  git branch -D $b"; done

echo
# `read -r -p "prompt" VARNAME` prints the prompt, waits for you to type
# something and press Enter, then stores what you typed into $CONFIRM.
#   -r  "raw" mode - treats backslashes literally instead of as escape
#       characters, which is the safer default for reading free-text input
#   -p  the prompt text to display before waiting for input
CONFIRM=""
read -r -p "Type 'yes' to proceed, anything else to cancel: " CONFIRM

# We check for the EXACT string "yes" - anything else (including "Yes",
# "y", or just pressing Enter) cancels. This is deliberate: it should be
# hard to trigger deletion by accident.
if [ "$CONFIRM" != "yes" ]; then
  echo "Cancelled. No changes made."
  exit 0
fi

echo
echo "Deleting Tier 1 remote branches..."
FAILED_REMOTE=()
for b in "${TIER1[@]}"; do
  name="${b#origin/}"   # strip the "origin/" prefix - `git push --delete`
                        # wants just the branch name, not the remote-
                        # tracking ref form
  echo "  -> git push origin --delete $name"

  # `git push origin --delete <branch>` deletes <branch> ON THE REMOTE
  # (this is different from deleting your local copy of it).
  #
  # `if ! command; then ...` runs the command and checks its exit code:
  # `!` negates it, so this block only runs if the command FAILED (e.g.
  # it hit branch protection, like the GH006 error from earlier, or the
  # branch was already deleted by someone else in the meantime). We
  # deliberately don't let one failure stop the whole loop - we note it
  # and move on to the next branch.
  if ! git push origin --delete "$name"; then
    echo "     FAILED (likely protected, or already gone) - skipping"
    FAILED_REMOTE+=("$name")
  fi
done

echo
echo "Deleting Tier 2 local branches..."
FAILED_LOCAL=()
for b in "${TIER2[@]}"; do
  echo "  -> git branch -D $b"

  # `git branch -D <branch>` force-deletes a LOCAL branch. We use the
  # capital -D (force) rather than lowercase -d here because these
  # branches are ones whose remote is already gone - git's normal safety
  # check for -d (which refuses to delete a branch with unmerged commits)
  # isn't meaningful for a branch that no longer has anywhere to be
  # merged TO. If you'd rather be extra cautious, you can change this to
  # -d and re-run - it'll simply fail safely on anything with unmerged
  # work instead of deleting it.
  if ! git branch -D "$b"; then
    echo "     FAILED - skipping"
    FAILED_LOCAL+=("$b")
  fi
done

# Final summary: report anything that didn't go through, so nothing gets
# silently missed.
section "Cleanup complete"
if [ ${#FAILED_REMOTE[@]} -gt 0 ]; then
  echo "Could not delete remotely (check branch protection rules):"
  printf '  %s\n' "${FAILED_REMOTE[@]}"
fi
if [ ${#FAILED_LOCAL[@]} -gt 0 ]; then
  echo "Could not delete locally:"
  printf '  %s\n' "${FAILED_LOCAL[@]}"
fi
echo "Done."
