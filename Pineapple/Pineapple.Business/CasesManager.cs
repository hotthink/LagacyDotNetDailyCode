﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Pineapple.Data;
using Pineapple.Model;

namespace Pineapple.Business
{
    public class CasesManager
    {
        [Dependency]
        public ICasesData CasesData { protected get; set; }

        [Dependency]
        public AttachmentManager AttachmentManager { private get; set; }

        public Cases GetCase(int caseId)
        {
            Cases cases = CasesData.GetCase(caseId);
            if (cases != null)
            {
                cases.CaseItems = CasesData.LoadCaseItemsByCaseId(caseId);
                cases.CaseItems = cases.CaseItems ?? new List<CaseItem>(0);
                LoadCaseItemAttachment(cases.CaseItems);
            }
            return cases;
        }

        public List<Cases> LoadCases(Pagination pagination, CaseType caseType)
        {
            var cases = CasesData.LoadCases(pagination, caseType);
            LoadCaseItems(cases);
            if (cases != null && cases.Count > 0)
            {
                var items = new List<CaseItem>();
                cases.ForEach(c => { if (c.CaseItems != null)items.AddRange(c.CaseItems); });
                LoadCaseItemAttachment(items);
            }
            return cases ?? new List<Cases>(0);
        }

        public List<Cases> LoadSimpleCases(Pagination pagination, CaseType caseType)
        {
            var cases = CasesData.LoadCases(pagination, caseType);
            return cases ?? new List<Cases>(0);
        }

        public Cases SaveCase(Cases cases)
        {
            SetDefaultTime(cases);
            cases = CasesData.SaveCase(cases);

            if (cases.CaseItems != null && cases.CaseItems.Count > 0)
            {
                cases.CaseItems.ForEach(SetDefaultTime);
                cases.CaseItems = CasesData.SaveCaseItems(cases.CaseItems);
            }

            return cases;
        }

        public bool DeleteCase(int caseId)
        {
            return CasesData.DeleteCase(caseId);
        }

        private void LoadCaseItemAttachment(List<CaseItem> caseItems)
        {
            if (caseItems == null || caseItems.Count == 0) return;
            List<int> attachmentIds = caseItems.Select(i => i.AttachmentId).Distinct().ToList();
            List<Attachment> attachments = AttachmentManager.LoadAttachmentByIds(attachmentIds);
            if (attachments == null || attachments.Count == 0) return;

            Dictionary<int, Attachment> dicAttachment = new Dictionary<int, Attachment>(attachments.Count);
            attachments.ForEach(a => { if (!dicAttachment.ContainsKey(a.AttachmentId ?? -1))dicAttachment.Add(a.AttachmentId ?? -1, a); });

            foreach (var caseItem in caseItems)
            {
                if (dicAttachment.ContainsKey(caseItem.AttachmentId))
                {
                    caseItem.Attachment = dicAttachment[caseItem.AttachmentId];
                }
            }
        }

        private void LoadCaseItems(List<Cases> caseses)
        {
            if (caseses == null || caseses.Count == 0) return;
            List<int> caseIds = caseses.Select(c => c.CaseId).ToList();
            if (caseIds.Count == 0) return;

            List<CaseItem> items = CasesData.LoadCaseItemsByCaseIds(caseIds);
            var caseIdItems = new Dictionary<int, List<CaseItem>>(caseses.Count);
            items.ForEach(item =>
                              {
                                  if (caseIdItems.ContainsKey(item.CaseId)) caseIdItems[item.CaseId].Add(item);
                                  else caseIdItems.Add(item.CaseId, new List<CaseItem>() { item });
                              });

            var itemComparer = CaseItemDisplayOrderComparer.Instance;
            foreach (var c in caseses)
            {
                if (caseIdItems.ContainsKey(c.CaseId))
                {
                    c.CaseItems = caseIdItems[c.CaseId];
                    c.CaseItems.Sort(itemComparer);
                }
            }
        }

        public List<CaseItem> LoadCaseItemsByCaseId(int caseId)
        {
            var items = CasesData.LoadCaseItemsByCaseId(caseId);
            items.Sort(CaseItemDisplayOrderComparer.Instance);
            LoadCaseItemAttachment(items);
            return items;
        }

        public CaseItem SaveCaseItem(CaseItem item)
        {
            if (item == null) return item;
            if (item.CaseId <= 0) return item;
            SetDefaultTime(item);
            return CasesData.SaveCaseItems(new List<CaseItem>() { item }).First();
        }

        public bool DeleteCaseItem(int caseItemId)
        {
            return CasesData.DeleteCaseItem(caseItemId);
        }

        private void SetDefaultTime(CaseItem item)
        {
            if (item != null && item.CreateDate == DateTime.MinValue) item.CreateDate = DateTime.Now;
        }
        private void SetDefaultTime(Cases cases)
        {
            if (cases != null && cases.CreateDate == DateTime.MinValue) cases.CreateDate = DateTime.Now;
        }
    }
}
