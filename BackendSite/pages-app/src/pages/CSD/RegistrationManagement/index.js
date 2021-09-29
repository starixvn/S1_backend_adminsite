import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment-timezone";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
import Note from "./Note";

const App = () => {
  moment.tz.setDefault("Asia/Taipei");
  const [siteList, setSiteList] = useState([]);
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    Currency: 0,
    DepositStatus: 0,
    Status: 0,
    User: "",
    RegTimeType: 1,
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    AffiliatesCode:"",
    AgentCodeType:1
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });
  const [status, setstatus] = useState(0);
  const [noteFilter, setNoteFilter] = useState({
    Note: "",
    CustId: 0,
  });
  const [noteDatas, setnoteDatas] = useState([]);
  const [editNoteData, seteditNoteData] = useState({
    Id: 0,
    NewNote: "",
  });
  const [count, setCount] = useState(30);
  const [isloading, setisloading] = useState(true);
  const [editstatus, seteditstatus] = useState({
    CustId: 0,
  });

  useEffect(() => {
    let id = setInterval(() => {
      if (count > 0 && status === 0) {
        setCount(count - 1);
      }
    }, 1000);
    return () => clearInterval(id);
  });

  useEffect(() => {
    let param = {
      Url: "Common/AdminUserSiteList",
    };
    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        setSiteList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);

  const Filter_OnChange = (v, dataColumn) => {
    console.log(v, dataColumn);
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };

  const Submit = () => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    const siteIds = [];
    stateFilter.SelectSite.map((o) => {
      siteIds.push(o.value);
    });
    setisloading(true);
    let param = {
      Url: "CustManage/GetRegLiveStatusData",
      Data: {
        SiteIds: siteIds,
        Currency: Number(stateFilter.Currency),
        DepositStatus: Number(stateFilter.DepositStatus),
        Status: Number(stateFilter.Status),
        User: stateFilter.User,
        Sort: Number(stateFilter.RegTimeType),
        PageSize: 30,
        PageNumber: 1,
        StartDate: moment(stateFilter.StartDate).unix(),
        EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
        AffiliatesCode:stateFilter.AffiliatesCode,
        AgentCodeType:Number(stateFilter.AgentCodeType)
      },
    };
    WebAPI.PostData(param)
      .then((res) => {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
            },
            FilterInfo: {
              ...stateFilter,
            },
          });
          setCount(30);
          setisloading(false);
        } else {
          setCount(30);
          setisloading(false);
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
    setCount(30);
    setisloading(false);
  };

  useEffect(() => {
    if (count === 0 && stateDatas.FilterInfo !== null) {
      setisloading(true);
      const siteIds = [];
      stateDatas.FilterInfo.SelectSite.map((o) => {
        siteIds.push(o.value);
      });
      let param = {
        Url: "CustManage/GetRegLiveStatusData",
        Data: {
          SiteIds: siteIds,
          Currency: Number(stateDatas.FilterInfo.Currency),
          DepositStatus: Number(stateDatas.FilterInfo.DepositStatus),
          Status: Number(stateDatas.FilterInfo.Status),
          User: stateDatas.FilterInfo.User,
          Sort: Number(stateDatas.FilterInfo.RegTimeType),
          PageSize: 30,
          PageNumber: Number(stateDatas.PageInfo.pageNumber),
          StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
          EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, "days").unix() - 1,
        },
      };
      WebAPI.PostData(param)
        .then((res) => {
          if (res.data.errorCode === 0) {
            setDatas({
              ...stateDatas,
              Datas: res.data.data,
              PageInfo: {
                totalPages: res.data.totalPages,
                pageNumber: stateDatas.PageInfo.pageNumber,
                rowCount: res.data.rowCount,
              },
            });
            setCount(30);
            setisloading(false);
          } else {
            alert(res.data.message ?? "failed to get data.");
            setCount(30);
            setisloading(false);
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  }, [count]);

  const Result_ExportToExcel = () => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    const siteIds = [];
    stateFilter.SelectSite.map((o) => {
      siteIds.push(o.value);
    });
    let param = {
      Url: "CustManage/ExportRegLiveStatus",
      Data: {
        SelectSite: JSON.stringify(siteIds),
        Currency: Number(stateFilter.Currency),
        DepositStatus: Number(stateFilter.DepositStatus),
        Status: Number(stateFilter.Status),
        User: stateFilter.User,
        Sort: Number(stateFilter.RegTimeType),
        StartDate: moment(stateFilter.StartDate).unix(),
        EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
        AffiliatesCode:stateFilter.AffiliatesCode,
        AgentCodeType:Number(stateFilter.AgentCodeType)
      },
    };

    WebAPI.DownloadData(param);
  };

  const Result_ChangePage = (PageNumber) => {
    setisloading(true);
    const siteIds = [];
    stateDatas.FilterInfo.SelectSite.map((o) => {
      siteIds.push(o.value);
    });
    let param = {
      Url: "CustManage/GetRegLiveStatusData",
      Data: {
        SiteIds: siteIds,
        Currency: Number(stateDatas.FilterInfo.Currency),
        DepositStatus: Number(stateDatas.FilterInfo.DepositStatus),
        Status: Number(stateDatas.FilterInfo.Status),
        User: stateDatas.FilterInfo.User,
        Sort: Number(stateDatas.FilterInfo.RegTimeType),
        PageSize: 30,
        PageNumber: PageNumber,
        StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
        EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, "days").unix() - 1,
        AffiliatesCode:stateDatas.AffiliatesCode,
        AgentCodeType:Number(stateDatas.AgentCodeType)
      },
    };
    WebAPI.PostData(param)
      .then((res) => {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: PageNumber,
              rowCount: res.data.rowCount,
            },
          });
          setCount(30);
          setisloading(false);
        } else {
          setCount(30);
          setisloading(false);
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const UpdateStatus = (status, custid, siteid) => {
    let Data = {
      CustId: Number(custid),
      SiteId: Number(siteid),
      Status: Number(status),
    };
    api
      .post("/api/CustManage/SetRegLiveStatus", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          UpdateReset();
          CancelChangeStatus();
        } else {
          alert(res.data.message);
          UpdateReset();
          CancelChangeStatus();
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const UpdateReset = () => {
    const siteIds = [];
    stateDatas.FilterInfo.SelectSite.map((o) => {
      siteIds.push(o.value);
    });
    let param = {
      Url: "CustManage/GetRegLiveStatusData",
      Data: {
        SiteIds: siteIds,
        Currency: Number(stateDatas.FilterInfo.Currency),
        DepositStatus: Number(stateDatas.FilterInfo.DepositStatus),
        Status: Number(stateDatas.FilterInfo.Status),
        User: stateDatas.FilterInfo.User,
        Sort: Number(stateDatas.FilterInfo.RegTimeType),
        PageSize: 30,
        PageNumber: stateDatas.PageInfo.pageNumber,
        StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
        EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, "days").unix() - 1,
      },
    };
    WebAPI.PostData(param)
      .then((res) => {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: stateDatas.PageInfo.pageNumber,
              rowCount: res.data.rowCount,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const EditNote = (custid) => {
    let param = {
      Url: "CustManage/GetRegLiveStatusNotes",
      Data: {
        CustId: Number(custid),
      },
    };
    WebAPI.PostData(param)
      .then((res) => {
        if (res.data.errorCode === 0) {
          setnoteDatas(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
    setstatus(1);
    setNoteFilter({
      ...noteFilter,
      CustId: custid,
    });
  };

  const ChangeStatus = (custid) => {
    seteditstatus({
      CustId: custid,
    });
  };
  const CancelChangeStatus = () => {
    seteditstatus({
      CustId: 0,
    });
  };

  const Back = () => {
    seteditNoteData({
      Id: 0,
      NewNote: "",
    });
    setNoteFilter({
      Note: "",
      CustId: 0,
    });
    setstatus(0);
  };

  const AddNote = (props) => {
    setNoteFilter({
      ...noteFilter,
      Note: props,
    });
  };
  const SubmitAddNote = () => {
    let Data = {
      CustId: Number(noteFilter.CustId),
      Note: noteFilter.Note,
    };
    api
      .post("/api/CustManage/AddRegLiveStatusNote", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          let param = {
            Url: "CustManage/GetRegLiveStatusNotes",
            Data: {
              CustId: Number(noteFilter.CustId),
            },
          };
          WebAPI.PostData(param)
            .then((res) => {
              if (res.data.errorCode === 0) {
                setnoteDatas(res.data.data);
              } else {
                alert(res.data.message ?? "failed to get data.");
              }
            })
            .catch(function (error) {
              alert(error);
            });
        } else {
          alert(res.data.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const EditButton = (id) => {
    seteditNoteData({
      Id: id,
      NewNote: "",
    });
  };
  const DeleteButton = (id) => {
    if (confirm("Are you sure Delete ?")) {
      let Data = {
        CustId: Number(noteFilter.CustId),
        Id: id,
      };
      api
        .post("/api/CustManage/DeleteRegLiveStatusNote", Data)
        .then(function (res) {
          if (res.data.errorCode === 0) {
            let param = {
              Url: "CustManage/GetRegLiveStatusNotes",
              Data: {
                CustId: Number(noteFilter.CustId),
              },
            };
            WebAPI.PostData(param)
              .then((res) => {
                if (res.data.errorCode === 0) {
                  setnoteDatas(res.data.data);
                } else {
                  alert(res.data.message ?? "failed to get data.");
                }
              })
              .catch(function (error) {
                alert(error);
              });
          } else {
            alert(res.data.message);
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };
  const EditNewNote = (props) => {
    seteditNoteData({
      ...editNoteData,
      NewNote: props,
    });
  };
  const SaveButton = () => {
    let Data = {
      CustId: Number(noteFilter.CustId),
      Id: editNoteData.Id,
      Note: editNoteData.NewNote,
    };
    api
      .post("/api/CustManage/EditRegLiveStatusNote", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          let param = {
            Url: "CustManage/GetRegLiveStatusNotes",
            Data: {
              CustId: Number(noteFilter.CustId),
            },
          };
          WebAPI.PostData(param)
            .then((res) => {
              if (res.data.errorCode === 0) {
                setnoteDatas(res.data.data);
                seteditNoteData({
                  Id: 0,
                  NewNote: "",
                });
              } else {
                alert(res.data.message ?? "failed to get data.");
              }
            })
            .catch(function (error) {
              alert(error);
            });
        } else {
          alert(res.data.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const CancelButton = () => {
    seteditNoteData({
      Id: 0,
      NewNote: "",
    });
  };
  const Refresh = () => {
    setCount(0);
  };
  return status === 0 ? (
    <div className="container-fluid">
      <h4 className="page-name">Registration Management</h4>
      <Filter
        SiteList={siteList}
        filterInfo={stateFilter}
        onChange={Filter_OnChange}
        ClickSubmit={Submit}
      />
      {stateDatas.Datas === null ? null : (
        <Result
          {...stateDatas}
          UpdateStatus={UpdateStatus}
          ChangePage={Result_ChangePage}
          EditNote={EditNote}
          Result_ExportToExcel={Result_ExportToExcel}
          Isloading={isloading}
          Count={count}
          Refresh={Refresh}
          Editstatus={editstatus}
          ChangeStatus={ChangeStatus}
          CancelChangeStatus={CancelChangeStatus}
        />
      )}
    </div>
  ) : (
    <div>
      <Note
        AddNote={AddNote}
        SubmitAddNote={SubmitAddNote}
        NoteDatas={noteDatas}
        EditButton={EditButton}
        DeleteButton={DeleteButton}
        SaveButton={SaveButton}
        CancelButton={CancelButton}
        EditNoteData={editNoteData}
        EditNewNote={EditNewNote}
        Back={Back}
      />
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
