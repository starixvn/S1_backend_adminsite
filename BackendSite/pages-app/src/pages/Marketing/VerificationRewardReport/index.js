import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import moment from "moment";
import WebAPI from "../../../module//WebAPI";
import Filter from "./Filter";
import Result from "./Result";

const App = () => {
    const dateformat = "YYYY-MM-DD";
    const [stateFilter, setFilter] = useState({
        siteIds: [1,2,3,4],
        startDate: moment().subtract(10, "days").format(dateformat),
        endDate: moment().format(dateformat),
        userName: "",
        currency: 0,
        dateQuickPicker: 0,
    });
    const [datas, setDatas] = useState({
        PageInfo: null,
        Datas: null
      });
    const [siteList, setSiteList] = useState([]);

    useEffect(() => {
      WebAPI.PostData({ Url: "Common/AdminUserSiteList" }).then((res) => {
        if (res.data.errorCode === 0) {
          let siteTmp = res.data.data.map((item) => {
            return { siteId: item.siteId, siteName: item.siteName, checked: true }
          })
          siteTmp.unshift({ siteId:0, siteName: "All", checked: true })
          setSiteList(siteTmp);
        } 
        else {
          alert(res.data.message ?? "failed to get data.");
        }
      });
      getReport(stateFilter)
    }, []);

    const getReport = (filter, pageNumber = 1, pageSize = 10) => {
      if (filter.siteIds.length === 0) {
        alert("Please Choose at least one Site");
        return false;
      }
      let query = {
        SiteIds: filter.siteIds,
        UserName: filter.userName === "" ? null : filter.userName,
        CurrencyId: filter.currency,
        StartDate: moment(filter.startDate).utc(),
        EndDate: moment(filter.endDate).utc(),
        PageNumber: pageNumber,
        PageSize: pageSize
      };
  
      let param = {
        Url: "VerificationRewardReport/Get",
        Data: query,
      };
  
      WebAPI.PostData(param).then((res) => {
  
        if (res.data.errorCode === 0) {
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: res.data.pageNumber,
              rowCount: res.data.rowCount
            },
            Datas: [...res.data.data]
          });
        } else {
          alert(res.data.message);
        }
      });
    }

    const exportReport = (filter) => {
      let query = {
        SiteIds: filter.siteIds,
        UserName: filter.userName === "" ? null : filter.userName,
        CurrencyId: filter.currency,
        StartDate: moment(filter.startDate).utc(),
        EndDate: moment(filter.endDate).utc(),
      };
  
      let param = {
        Url: "VerificationRewardReport/Export",
        Data: query,
      };
      WebAPI.DownloadData(param);
    }

    const resultChangePage = (pageNumber) => {
      getReport(stateFilter, pageNumber);
    };

    const datePickerOnChange = (value) => {
      console.log(value)
      let _startDate = "";
      let _endDate = "";
  
      switch (value) {
        case "1":
          _startDate = moment().startOf("day");
          _endDate = moment().endOf("day");
  
          break;
        case "2":
          _startDate = moment().subtract(1, "days");
          _endDate = moment().subtract(1, "days");
  
          break;
        case "3":
          _startDate = moment().startOf("week");
          _endDate = moment().endOf("week");
  
          break;
        case "4":
          _startDate = moment().subtract(1, "week").startOf("week");
          _endDate = moment().subtract(1, "week").endOf("week");
  
          break;
        case "5":
          _startDate = moment().startOf("month");
          _endDate = moment().endOf("month");
  
          break;
        case "6":
          _startDate = moment().subtract(1, "month").startOf("month");
          _endDate = moment().subtract(1, "month").endOf("month");
  
          break;
        default:
          break;
      }
      if (value !== "0") {
        setFilter({
          ...stateFilter,
          startDate: _startDate.format(dateformat),
          endDate: _endDate.format(dateformat),
          dateQuickPicker: value
        });
      }else{
        setFilter({
          ...stateFilter,
          dateQuickPicker: value
        });
      }
    };

    const siteCheck = (siteId, checked) => {
      if(siteId == 0) {
        siteList.forEach( a => { a.checked = !checked })
      }
      else {
        let idx = siteList.map( i => {
          return i.siteId;
        }).indexOf(siteId)
        siteList[idx].checked = !checked
      }
  
      let siteIds = []
      siteList.forEach(a => {
        if(a.siteId !=0 && a.checked)
          siteIds.push(a.siteId)
      })
      setFilter({
        ...stateFilter,
        siteIds: siteIds,
      });
      setSiteList([...siteList])
    }

    const filterChange = (v, dataColumn) => {
      console.log(v, dataColumn)
      setFilter({
        ...stateFilter,
        [dataColumn]: v,
      });
    }

    const search = () => {
      getReport(stateFilter);
    }

    return (
        <div className="container-fluid">
            <h4 className="page-name">Verification Reward Report</h4>
            <Filter 
                siteList={siteList} 
                filterInfo={stateFilter}
                siteChecked={siteCheck}
                onChange={filterChange}
                search={search}
                datePickerOnChange={datePickerOnChange} />
            {datas.Datas === null ? null : (
              <Result
                  {...datas}
                  ChangePage={resultChangePage}
                  ExportToExcel={exportReport}/>
          )}
        </div>
    );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}