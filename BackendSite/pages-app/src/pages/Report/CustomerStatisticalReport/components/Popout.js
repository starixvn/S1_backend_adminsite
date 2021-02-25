import React from "react";

const dateDataGroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'TurnOver(ValidBet)',
            colSpan: props.CurrencyList.length + 1,
            body:{
                attr:{
                    bgcolor:"Bisque"
                },
                style:{
                    position: "sticky",
                    top: "0",
                    left:"0",
                    height:"36.8px",
                    zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "0",
                left:"0",
                height:"36.8px",
                zIndex:"3"
            }
        },
        {
            columnTitle: 'WinLoss',
            colSpan: props.CurrencyList.length + 1,
            body:{
                attr:{
                    bgcolor:"Bisque"
                },
                style:{
                    position: "sticky",
                    top: "0",
                    left:"0",
                    height:"36.8px",
                    zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "0",
                left:"0",
                height:"36.8px",
                zIndex:"3"
            }
        }
    ]
}

const dateDataColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Date',
            dataColumn: "date",
            body:{
                attr:{
                    bgcolor:"Bisque"
                },
                style:{
                    position: "sticky",
                    top: "36.8px",
                    left:"0",
                    height:"36.8px",
                    zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "36.8px",
                left:"0",
                height:"36.8px",
                zIndex:"3"
            }
        },
        {
            columnTitle: 'Category Name',
            dataColumn: "categoryName",
            body:{
                attr:{
                    bgcolor:"Bisque"
                },
                style:{
                    position: "sticky",
                    top: "36.8px",
                    left:"0",
                    height:"36.8px",
                    zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "36.8px",
                left:"0",
                height:"36.8px",
                zIndex:"3"
            }
        },
        {
            columnTitle: 'Game',
            dataColumn: "gameName",
            body:{
                attr:{
                    bgcolor:"Bisque"
                },
                style:{
                    position: "sticky",
                    top: "36.8px",
                    left:"0",
                    height:"36.8px",
                    zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "36.8px",
                left:"0",
                height:"36.8px",
                zIndex:"3"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `turnOverValidBet_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        hidden:props.HideShowBtn,
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "36.8px",
                    height:"36.8px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "turnOverValidBetSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `winLoss_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right"
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "36.8px",
                    height:"36.8px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "winLossSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        }
    ]
}

const Modal = ({handleClose, show, userdata,dateDatas,userTotalDatas}) => {
   console.log(userTotalDatas);

   let dataGroupHeaderRows=[];
   let dataHeaderRows = [];
   let bodyRows = [];
   let bodyTotalRows = [];

if(show)
{
   let dataGroupColumnDefs = dateDataGroupColumnDefs(userdata);
   let dataColumnDefs = dateDataColumnDefs(userdata);

   dataGroupColumnDefs.forEach((element, idx) => {
    dataGroupHeaderRows.push(
        <th key={`th-${element.columnTitle}-${idx}`}
            scope="col"
            colSpan={element.colSpan}
            style={element.style ? element.style : ''}>
            {element.columnTitle}
            {/* <button type="button" className="btn btn-info pull-right" onClick={() => props.HideShowBtn(element)}>{element.btnText}</button> */}
        </th>
    );
   });

   dataColumnDefs.forEach((element, idx) => {
    dataHeaderRows.push(
        <th key={`th-${element.dataColumn}-${idx}`} 
            scope="col"
            style={element.style ? element.style : ''}>
            {element.columnTitle}
        </th>
    );
   });

   bodyRows = dateDatas.map((data, idx) => {
       if(data.custId===userdata.CustId)
       {
        let row = dataColumnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <td key={`td-${element.dataColumn}-${idx}`}
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    style={element.body && element.body.style ? element.body.style : ''}
                    bgcolor={element.body && element.body.attr && element.body.attr.bgcolor ? element.body.attr.bgcolor : ''}
                   >
                    {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                </td>
            );
        });
    
        return (
            <tr key={`result-body-${idx}`}>
                {row}
            </tr>
        )
       }
});

bodyTotalRows = userTotalDatas.map((data, idx) => {
    if(data.custId===userdata.CustId)
    {
     let row = dataColumnDefs.map((element, idx) => {
         let subData = {};
         if (element.body && element.body.subData) {
             element.body.subData.forEach(s => { subData[s] = data[s] });
         }
         return (
             <th key={`td-${element.dataColumn}-${idx}`}
                 className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                 style={element.body && element.body.style ? element.body.style : ''}
                 bgcolor="LemonChiffon"
                >
                 {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
             </th>
         );
     });
 
     return (
         <tr key={`result-body-${idx}`}>
             {row}
         </tr>
     )
    }
});

}
    const showHideStyle = show ? {display:"block",background:"rgba(0, 0, 0, 0.6)"} : {};

  return (
    <div className="modal" style={showHideStyle}>
      <div style={{position:"absolute",width:"90%",height:"90%",background:"#ffffff",left:"5%",top:"5%",padding:"10px"}}>
            <div style={{background: "#E9E9E9",borderBottom: "0",padding:"8px",display:"flex",border:"1px solid #d5d5d5",height:"49px"}}>
                <span style={{fontWeight:"bold"}}>This is User <span style={{color:"red"}}>{userdata.UserName}</span> data by date.</span>
                <button type="button" className="btn btn-approved" style={{position:"absolute",right:"31px"}} onClick={handleClose}>Close</button>
            </div>
            <div style={{overflow:"scroll",height:"85%"}}>
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3"}}></th>
                            <th style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3"}}></th>
                            <th style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3"}}></th>
                            {dataGroupHeaderRows}
                        </tr>
                        <tr>
                            {dataHeaderRows}
                        </tr>
                    </thead>
                    <tbody>
                        {bodyRows}
                        {bodyTotalRows}
                    </tbody>
                </table>
            </div>
        </div>
    </div>
  );
};

export default Modal;
