import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import Edit from "./Edit"
import Result from "./Result";
import qs from "qs";
import WebAPI from "../../../module//WebAPI";
import moment from "moment-timezone";

const App = () => {

  const [eventState, updateEvent] = useState({
    ShowSettingPanel: false,
    IsAdd: false,
  });
  
  const [datas, setDatas] = useState({
    PageInfo: null,
    Datas: null
  });

  useEffect(() => {
    GetDisableUserList();
  }, [eventState]);

  const GetDisableUserList = (PageNumber = 1, PageSize = 10) => {

    let query = {
      siteIds: [1,2,3],
      UserName: null,
      StartDate: moment.utc('2021/08/25 00:00:00').unix(),
      EndDate: moment.utc('2020/08/31 00:00:00').unix(),
      PageNumber: PageNumber,
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
    // if (stateFilter.SelectSite.length === 0) {
    //   alert("Please Choose at least one Site");
    //   return false;
    // }
    GetDisableUserList(PageNumber);
  };

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
      } else {
        alert(res.data.message);
      }
    });
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">Disable User For Game</h4>
      {eventState.ShowSettingPanel ? 
        <Edit 
          ClickBack={() => {
            updateEvent({
              ...eventState,
              ShowSettingPanel: false,
            });
          }}
          InsertOrUpdate={UpdateDisableUserForGame}/> : 
      <>
        <button
          className="btn btn-add"
          style={{ marginBottom: "20px" }}
          onClick={() => {
              updateEvent({
                ...eventState,
                ShowSettingPanel: true,
                IsAdd: true,
              });
          }}>
          Add Maintenance
        </button>
        {datas.Datas === null ? null : (
            <Result
            {...datas}
            ChangePage={Result_ChangePage}
            />
          )}
      </>}
    </div>
  );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}