import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import Edit from "./Edit"
import Result from "./Result";
import Filter from "./Filter";
import qs from "qs";
import WebAPI, { api } from "../../../module//WebAPI";
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

    GetUserBetLimitData(stateFilter);
  }, []);

  const GetUserBetLimitData = (filter, pageNumber = 1, pageSize = 10) => {
    let query = {
      SiteIds: filter.SiteId,
      UserName: filter.UserName,
      StartDate: moment(filter.StartDate).utc().unix(),
      EndDate: moment(filter.EndDate).utc().unix(),
      pageNumber: pageNumber,
      PageSize: pageSize
    };

    let param = {
      Url: "UserBetLimit/Get",
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

  const InsertOrUpdate = (customer, betSetting) => {

    let param = {
      siteId: customer.siteId,
      currencyId: customer.currencyId,
      after: {
        userName: customer.userName,
        betSetting: betSetting
      }
    };
    
    api
      .post("/apiv2/TpBetLimit/Set", param)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          alert("success.");
          updateEvent({
            ...eventState,
            ShowSettingPanel: false,
          });
          GetUserBetLimitData(stateFilter);
        } else {
            alert(res.data.message ?? "failed to get data.");
          }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const Result_ChangePage = (PageNumber) => {
    GetUserBetLimitData(stateFilter, PageNumber);
  };

  const EditUser = (custId) => {
    updateEvent({
      ...eventState,
      ShowSettingPanel: true,
      IsAdd: false,
      CustId: custId
    });
  }

  const DeleteUser = (data) => {
    let param = {
      siteId: data.siteId,
      currencyId: data.currencyId,
      before: {
        custId: data.custId,
        userName: data. userName
      }
    };

    api
      .post("/apiv2/TpBetLimit/Delete", param)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          alert("success.");
          GetUserBetLimitData(stateFilter);
        } else {
            alert(res.data.message ?? "failed to get data.");
          }
      })
      .catch(function (error) {
        alert(error);
      });
  }

  const SwitchStatus = (custId, status) => {
    datas.Datas.forEach(a => {
      if(a.custId === custId) {
        a.status ^= 1
      }
    });
    setDatas({
      ...datas,
    });

    let data = {
      CustId: custId,
      Status: status ^= 1,
    };
    let param = {
      Url: "UserBetLimit/UpdateStatus",
      Data: qs.stringify(data),
    };

    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        alert("success.");
      } else {
        alert(res.data.message);
      }
    });
  }

  const FilterChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  }

  const SiteChange = (siteId, siteName, checked) => {
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
    GetUserBetLimitData(stateFilter);
  }

  return (
    <div className="container-fluid">
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
          InsertOrUpdate={InsertOrUpdate} /> :
        <>
        <h4 className="page-name">User Bet Limit</h4>
          <Filter
            siteList={siteList}
            filterInfo={stateFilter}
            onChange={FilterChange}
            siteChecked={SiteChange}
            search={Search} />
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
            Create
          </button>
          {datas.Datas === null ? null : (
              <Result
              {...datas}
              ChangePage={Result_ChangePage}
              ClickEdit={EditUser}
              ClickDelete={DeleteUser}
              UpdateStatus={SwitchStatus} />
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