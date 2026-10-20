using System.Collections.Generic;
using NUnit.Framework;
using VacuumSurvivor;

public class HighScoreTableTests
{
    static HighScoreEntry E(int score, int level = 0) =>
        new HighScoreEntry { score = score, seconds = 0, level = level, date = "" };

    [Test]
    public void Insert_into_empty_list()
    {
        var result = HighScoreTable.Insert(new List<HighScoreEntry>(), E(100));
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(100, result[0].score);
    }

    [Test]
    public void Sorts_descending_by_score()
    {
        var table = new List<HighScoreEntry>();
        table = HighScoreTable.Insert(table, E(50));
        table = HighScoreTable.Insert(table, E(200));
        table = HighScoreTable.Insert(table, E(100));
        CollectionAssert.AreEqual(new[] { 200, 100, 50 }, table.ConvertAll(e => e.score));
    }

    [Test]
    public void Trims_to_five_keeping_the_highest()
    {
        var table = new List<HighScoreEntry>();
        foreach (var s in new[] { 10, 20, 30, 40, 50, 60 }) table = HighScoreTable.Insert(table, E(s));
        Assert.AreEqual(5, table.Count);
        CollectionAssert.AreEqual(new[] { 60, 50, 40, 30, 20 }, table.ConvertAll(e => e.score));
    }

    [Test]
    public void Tie_keeps_existing_ahead_of_new()
    {
        var table = new List<HighScoreEntry> { E(100, level: 1) };
        table = HighScoreTable.Insert(table, E(100, level: 2));
        Assert.AreEqual(1, table[0].level, "existing row should stay ahead on a tie");
        Assert.AreEqual(2, table[1].level);
    }
}
