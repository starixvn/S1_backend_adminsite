import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import Edit from "./Edit"
import Result from "./Result";
import Filter from "./Filter";
import qs from "qs";
import WebAPI from "../../../module//WebAPI";
import moment from "moment";

const App = () => {
  const dateformat = "YYYY-MM-DD";
  const [eventState, updateEvent] = useState({
    ShowSettingPanel: false,
    IsAdd: true,
    CustId: 0
  });
  
  const [datas, setDatas] = useState({
    PageInfo: null,
    Datas: null
  });

  const [stateFilter, setFilter] = useState({
    SiteId: [1,2,3,4],
    StartDate: moment().add(-10, "days").format(dateformat),
    EndDate: moment().format(dateformat),
    UserName: ""
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
    GetDisableUserList(stateFilter);
  }, []);

  const GetDisableUserList = (filter, pageNumber = 1, PageSize = 10) => {
    if (filter.SiteId.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let query = {
      SiteIds: filter.SiteId,
      UserName: filter.UserName,
      StartDate: moment.utc(filter.StartDate).unix(),
      EndDate: moment.utc(filter.EndDate).unix(),
      pageNumber: pageNumber,
      PageSize: PageSize
    };
    let param = {
      Url: "Risk/GetTpDisableUser",
      Data: qs.stringify(query),
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

  const Result_ChangePage = (PageNumber) => {
    GetDisableUserList(stateFilter, PageNumber);
  };

  const EditDisableUser = (custId) => {
    updateEvent({
      ...eventState,
      ShowSettingPanel: true,
      IsAdd: false,
      CustId: custId
    });
  }

  const FilterChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  }

  const SiteChange = (siteId, checked) => {
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
    siteList.forEach( a => {
      if(a.siteId !=0 && a.checked)
      siteIds.push(a.siteId)
    })
                  
    setFilter({
      ...stateFilter,
      ["SiteId"]: siteIds,
    });
    setSiteList([...siteList])
  }

  const Search = () => {
    GetDisableUserList(stateFilter);
  }

  const UpdateDisableUserForGame = (userName, siteId, disableTpGame) => {

    let data = {
      UserName: userName,
      SiteId: siteId,
      DisableTpGame: disableTpGame
    };
    let param = {
      Url: "Risk/TpDisableUserForGame",
      Data: qs.stringify(data),
    };
    
    WebAPI.PostData(param).then((res) => {

      if (res.data.errorCode === 0) {
        alert("success.");
        updateEvent({
          ...eventState,
          ShowSettingPanel: false,
        });
        GetDisableUserList();
      } else {
        alert(res.data.message);
      }
    });
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">Disable User For Game</h4>
      <Filter 
        siteList={siteList} 
        filterInfo={stateFilter} 
        onChange={FilterChange}
        siteChecked={SiteChange}
        search={Search} />
      {eventState.ShowSettingPanel ? 
        <Edit 
          ClickBack={() => {
            updateEvent({
              ...eventState,
              ShowSettingPanel: false,
            });
          }}
          IsAdd={eventState.IsAdd}
          CustId = {eventState.CustId}
          InsertOrUpdate={UpdateDisableUserForGame} /> : 
        <>
          <button
            className="btn btn-add"
            style={{ marginBottom: "20px" }}
            onClick={() => {
                updateEvent({
                  ...eventState,
                  ShowSettingPanel: true,
                  IsAdd: true,
                  CustId: 0
                });
            }}>
            Add Disable User
          </button>
          {datas.Datas === null ? null : (
              <Result
              {...datas}
              ChangePage={Result_ChangePage}
              ClickEdit={EditDisableUser} />
          )}
        </>
      }
    </div>
  );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}