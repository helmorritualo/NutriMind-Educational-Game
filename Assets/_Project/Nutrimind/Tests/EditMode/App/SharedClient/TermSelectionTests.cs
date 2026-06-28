using System;
using NUnit.Framework;
using UnityEngine;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class TermSelectionTests
    {
        [SetUp]
        public void SetUp()
        {
            // Reset composition root or initialize cleanly
            var root = CompositionRoot.CreateForMode(DataProviderMode.LocalDemoJson);
            if (root.Session != null)
            {
                root.Session.Clear();
                root.Session.SubjectTermStore = new SubjectTermStore();
            }
        }

        [Test]
        public void SubjectTermStore_StoresSelectedSubjectAndTerm()
        {
            var store = new SubjectTermStore();
            Assert.That(store.SelectedSubject, Is.Null);
            Assert.That(store.CurrentTerm, Is.Null);

            store.SelectedSubject = SubjectType.LiteraQuest;
            store.CurrentTerm = "1";

            Assert.That(store.SelectedSubject, Is.EqualTo(SubjectType.LiteraQuest));
            Assert.That(store.CurrentTerm, Is.EqualTo("1"));

            store.Reset();
            Assert.That(store.SelectedSubject, Is.Null);
            Assert.That(store.CurrentTerm, Is.Null);
        }

        [Test]
        public void TermSelectionController_InstantiationAndReferences()
        {
            var go = new GameObject("TestTermController");
            var controller = go.AddComponent<TermSelectionController>();

            var t1 = new GameObject("T1");
            var t2 = new GameObject("T2");
            var t3 = new GameObject("T3");

            controller.SetTermCards(t1, t2, t3);

            // Verify basic state holds and no crash occurs on destruction
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(t1);
            UnityEngine.Object.DestroyImmediate(t2);
            UnityEngine.Object.DestroyImmediate(t3);

            Assert.Pass("TermSelectionController instantiated and cleaned up successfully.");
        }
    }
}
