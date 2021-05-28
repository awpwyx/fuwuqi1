using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;

using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core;
namespace SF.K3Cloud.Report.Plugin
{
    //简单账表1服务器插件
    [Description("穗丰测试报表")]
    [Kingdee.BOS.Util.HotUpdate]
    public class SFTest1 : SysReportBaseService
    {
        public override void Initialize()
        {
            base.Initialize();
            // 简单账表类型：普通、树形、分页            
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            // 报表名称            
            this.ReportProperty.ReportName = new LocaleValue("测试报表", base.Context.UserLocale.LCID);
            //             
            this.IsCreateTempTableByPlugin = true;            // 不利用动态构建列            
            this.ReportProperty.IsUIDesignerColumns = false;
            //             
            this.ReportProperty.IsGroupSummary = true;
            //             
            this.ReportProperty.SimpleAllCols = false;
            // 单据主键：两行FID相同，则为同一单的两条分录，单据编号可以不重复显示            
            this.ReportProperty.PrimaryKeyFieldName = "fid";
            //             
            this.ReportProperty.IsDefaultOnlyDspSumAndDetailData = true;
            // 报表主键字段名：默认为FIDENTITYID，可以修改            
            //this.ReportProperty.IdentityFieldName = "FIDENTITYID";        
        }
        public override string GetTableName()
        {
            var result = base.GetTableName();
            return result;
        }
        /// <summary>        
        /// 向报表临时表，插入报表数据        
        /// </summary>        
        /// <param name="filter"></param>        
        /// <param name="tableName"></param>        
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            SortedList<int, string> list = new SortedList<int, string>();//存储过滤条件            
            DynamicObject customFil = filter.FilterParameter.CustomFilter;//获取快捷页面的参数
            string sqlwhere = "0=0";
            string depid = customFil["F_ora_Base_Id"].ToString();//部门
            string projectid = customFil["F_ora_Base1_Id"].ToString();//工程项目
            string begintime = "0";
            string endtime = "0";
            object staffObjj0 = customFil["F_ora_Date"];
            object staffObjj1 = customFil["F_ora_Date1"];
            if (  staffObjj0 != null)
            {
                
                endtime = customFil["F_ora_Date1"].ToString();//结束日期
            }
            if (staffObjj1 != null)
            {
                
                endtime = customFil["F_ora_Date1"].ToString();//结束日期
            }

            //时间提取，第一页面进行对比；小数点保留两位；工程款录入程序写入时间（sql时间数据应与开发平台时间格式相同）
            //DynamicObject staffObjj4 = staffObjjO[23] as DynamicObject;
            if (depid != "0")
            {
                sqlwhere = sqlwhere + "and T_BD_DEPARTMENT.FDEPTID= " + "'" + depid + "'";
            }

            if (projectid != "")
            {
                sqlwhere = sqlwhere + "and T_BAS_ASSISTANTDATAENTRY.FENTRYID=" + "'" + projectid + "'";
            }
            if (begintime == "0")
            {
                begintime = "2000-01-01";
            }
            if (endtime == "0")
            {
                endtime = "9999-01-01";
            }
            //if (begintime != "0" && endtime != "0")
            //{
            //    sqlwhere = sqlwhere + "and T_JX_Budget.FInsertTime > '" + begintime + "' and convert(varchar(10),T_JX_Budget.FInsertTime, 120)<'" + endtime + "'";
            //}
            // 默认排序字段：需要从filter中取用户设置的排序字段            
            string seqFld = string.Format(base.KSQL_SEQ, "fid");
            string sql = string.Format(@"/*dialect*/select ROW_NUMBER()over(order by T_BAS_ASSISTANTDATAENTRY_L.FDATAVALUE) as fid,
T_BAS_ASSISTANTDATAENTRY_L.FDATAVALUE,convert(decimal(18,2),sum(T_JX_Budget.FSumPrice)/10000) as FSumPrice ,min(T_JX_Budget.FCreateTime)as FCreateTime ,min(CONVERT(varchar(10),T_JX_Budget.FInsertTime,111))as FInsertTime,min(FProjectNumber)as fprojecnumber,ROW_NUMBER()over(order by T_BAS_ASSISTANTDATAENTRY_L.FDATAVALUE) as FIDENTITYID ,'{3}' as 'Fbegintime','{4}' as 'Fendtime' into {1}   from T_JX_Budget
left join T_BAS_ASSISTANTDATAENTRY on T_JX_Budget.FProjectNumber=T_BAS_ASSISTANTDATAENTRY.FNUMBER and T_BAS_ASSISTANTDATAENTRY.FID = '573449f4900500'
left join T_BAS_ASSISTANTDATAENTRY_L 
on T_BAS_ASSISTANTDATAENTRY_L.FENTRYID=T_BAS_ASSISTANTDATAENTRY.FENTRYID
left join T_BD_DEPARTMENT 
on T_BD_DEPARTMENT.FNUMBER=T_JX_Budget.FDepartment
where {2}
group by FDATAVALUE
",
seqFld, tableName, sqlwhere,begintime.ToString(),endtime.ToString());
            DBUtils.ExecuteDynamicObject(this.Context, sql.ToString());
        }
        /// <summary>        
        /// 构建报表列        
        /// </summary>        
        /// <param name="filter"></param>        
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();
            // 订单编号            
            var fprojecnumber = header.AddChild("fprojecnumber", new LocaleValue("项目编码"));
            fprojecnumber.Mergeable = true;
            fprojecnumber.Visible = false;
            fprojecnumber.Index = 0;
            // 客户编码            
            var fid = header.AddChild("fid", new LocaleValue("序号"));
            fid.Mergeable = true;
            fid.Index = 1;
            // 客户名称            
            var FDATAVALUE = header.AddChild("FDATAVALUE", new LocaleValue("项目名称"));
            FDATAVALUE.Mergeable = true;
            FDATAVALUE.Index = 2;
            // 客户名称            
            var FSumPrice = header.AddChild("FSumPrice", new LocaleValue("总金额（单位：万元）"));
            FSumPrice.Mergeable = true;
            FSumPrice.Index = 3;
            // 客户名称            
            var FInsertTime = header.AddChild("FInsertTime", new LocaleValue("项目时间"));
            FInsertTime.Mergeable = true;
            FInsertTime.Index = 4;
            // 客户名称            
            var Fbegintime = header.AddChild("Fbegintime", new LocaleValue("开始时间"));
            Fbegintime.Mergeable = true;
            Fbegintime.Visible = false;
            Fbegintime.Index = 5;
            // 客户名称            
            var Fendtime = header.AddChild("Fendtime", new LocaleValue("结束时间"));
            Fendtime.Mergeable = true;
            Fendtime.Visible = false;
            Fendtime.Index = 6;

            return header;
        }


        protected override string GetSummaryColumsSQL(List<SummaryField> summaryFields)
        {
            var result = base.GetSummaryColumsSQL(summaryFields);
            return result;
        }
        protected override System.Data.DataTable GetListData(string sSQL)
        {
            var result = base.GetListData(sSQL);
            return result;
        }
        protected override System.Data.DataTable GetReportData(IRptParams filter)
        {
            var result = base.GetReportData(filter);
            return result;
        }
        protected override System.Data.DataTable GetReportData(string tablename, IRptParams filter)
        {
            var result = base.GetReportData(tablename, filter);
            return result;
        }
        public override int GetRowsCount(IRptParams filter)
        {
            var result = base.GetRowsCount(filter);
            return result;
        }
        public override void CloseReport()
        {
            base.CloseReport();
        }
        protected override void CreateTempTable(string sSQL)
        {
            base.CreateTempTable(sSQL);
        }
        public override void DropTempTable()
        {
            base.DropTempTable();
        }
    }
}


