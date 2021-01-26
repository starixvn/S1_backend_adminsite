import React from "react";
import Modal from "./components/Popout";

//#region Summary Data Column Define

const dataGroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: '%',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: '%',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: '%',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Total Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Total Count',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Averange Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Success Rate(%)',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'FTD Count',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Averange Processing Time(d.HH:MM:SS)',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
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

const dataColumnDefs = (props) => {
    return [
        { 
            columnTitle: 'Detail',
            dataColumn: '',
            body:{
                attr:{
                    bgcolor:"Bisque",
                    class:"text-center",
                },
                style:{
                    position: "sticky",
                    top: "42.36px",
                    left:"0",
                    height:"36.8px",
                    zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "73.6px",
                left:"0",
                zIndex:"3"
            }
        },
        { 
            columnTitle: 'AccountName',
            dataColumn: 'accountName',
            body:{
                attr:{
                    bgcolor:"Bisque",
                    class:"text-center",
                },
                style:{
                    position: "sticky",
                    top: "42.36px",
                    left:"51.2px",
                    height:"36.8px",
                    zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "73.6px",
                left:"51.2px",
                zIndex:"3"
            }
        },
        //Online Bank
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `onlineSummary_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total In MYR',
            dataColumn: "onlineSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `onlinePrecent_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total %',
            dataColumn: "onlinePrecentSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Mobile Bank
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `mobileSummary_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total In MYR',
            dataColumn: "mobileSummary",
            
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `mobilePrecent_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total %',
            dataColumn: "mobilePrecentSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
         //Local Bank
         ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `localSummary_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total In MYR',
            dataColumn: "localSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `localPrecent_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total %',
            dataColumn: "localPrecentSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Total Amount
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `totalAmount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Amount In MYR',
            dataColumn: "totalAmountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },

        //Total Count
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `totalCount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Count',
            dataColumn: "totalCountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Averange Amount
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `averangeAmount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Averange Amount IN MYR',
            dataColumn: "averangeAmountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Success Rate
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `successRate_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Success Rate',
            dataColumn: "successRateSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //FTD Count
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `ftdCount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total FTD Count',
            dataColumn: "ftdCountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Averange Processing Time
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `apTime_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Averange Processing Time',
            dataColumn: "apTimeSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
    ]
}

//#endregion

//#region Detail Data Column Define

const dateDataAccountColumnDefs = (DateData,props)=>{
    return [
        ...DateData.map(data=>{
            return({
                columnTitle: data.accountName,
                colSpan: (props.CurrencyList.length + 1)*5,
                attr:{
                    class:"text-center",
                },
                style:{
                    position: "sticky",
                    top: "0",
                    height:"36.8px",
                    zIndex:"2"
                }
            })
        }),
    ]
}

const dateDataGroupColumnDefs = (DateData,props) => {
    let dateGroupColumnResult=[];

    DateData.forEach(data=>{
        dateGroupColumnResult.push(
            
                {
                    columnTitle: 'Total Amount',
                    colSpan: props.CurrencyList.length + 1,
                    attr:{
                        class:"text-center",
                    },
                    style:{
                        position: "sticky",
                        top: "36.8px",
                        height:"36.8px",
                        zIndex:"2"
                    }
                },
                {
                    columnTitle: 'Total Count',
                    colSpan: props.CurrencyList.length + 1,
                    attr:{
                        class:"text-center",
                    },
                    style:{
                        position: "sticky",
                        top: "36.8px",
                        height:"36.8px",
                        zIndex:"2"
                    }
                },
                {
                    columnTitle: 'Averange Amount',
                    colSpan: props.CurrencyList.length + 1,
                    attr:{
                        class:"text-center",
                    },
                    style:{
                        position: "sticky",
                        top: "36.8px",
                        height:"36.8px",
                        zIndex:"2"
                    }
                },
                {
                    columnTitle: 'Success Rate(%)',
                    colSpan: props.CurrencyList.length + 1,
                    attr:{
                        class:"text-center",
                    },
                    style:{
                        position: "sticky",
                        top: "36.8px",
                        height:"36.8px",
                        zIndex:"2"
                    }
                },
                {
                    columnTitle: 'FTD Count',
                    colSpan: props.CurrencyList.length + 1,
                    attr:{
                        class:"text-center",
                    },
                    style:{
                        position: "sticky",
                        top: "36.8px",
                        height:"36.8px",
                        zIndex:"2"
                    }
                }
            
        );
    });

    return dateGroupColumnResult;

}

const dateDataColumnDefs = (DateData,props) => {
    let dateColResult=[];

    //Date Column
    dateColResult.push({
        columnTitle: 'Date',
        dataColumn: 'date',
        body:{
            attr:{
                bgcolor:"Bisque",
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "42.36px",
                left:"0",
                height:"36.8px",
                zIndex:"1"
            }
        },
        style:{
            position: "sticky",
            top: "73.6px",
            left:"0",
            zIndex:"3"
        }
    });
    
    //Other Column
    DateData.forEach(data=>{

        //Total Amount
        props.CurrencyList.forEach(curr => {

            dateColResult.push({
                columnTitle: `${curr.code}`,
                dataColumn: `totalAmount_${data.accountType}_${data.accountId}_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            });
        });
        dateColResult.push({
            columnTitle: 'Total Amount In MYR',
             dataColumn: `totalAmountSummary_${data.accountType}_${data.accountId}`,
             body:{
                 attr:{
                     class:"text-right",
                     bgcolor:"LightGray"
                 },
                 style:{}
             },
             style:{
                 position: "sticky", 
                 top: "73.6px",
                 zIndex:"2"
             }
        });

        //Total Count
        props.CurrencyList.forEach(curr => {

            dateColResult.push({
                columnTitle: `${curr.code}`,
                dataColumn: `totalCount_${data.accountType}_${data.accountId}_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            });
        });
        dateColResult.push({
            columnTitle: 'Total Count',
             dataColumn: `totalCountSummary_${data.accountType}_${data.accountId}`,
             body:{
                 attr:{
                     class:"text-right",
                     bgcolor:"LightGray"
                 },
                 style:{}
             },
             style:{
                 position: "sticky", 
                 top: "73.6px",
                 zIndex:"2"
             }
        });

        //Averange Amount
        props.CurrencyList.forEach(curr => {

            dateColResult.push({
                columnTitle: `${curr.code}`,
                dataColumn: `averangeAmount_${data.accountType}_${data.accountId}_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            });
        });
        dateColResult.push({
            columnTitle: 'Averange Amount In MYR',
             dataColumn: `averangeAmountSummary_${data.accountType}_${data.accountId}`,
             body:{
                 attr:{
                     class:"text-right",
                     bgcolor:"LightGray"
                 },
                 style:{}
             },
             style:{
                 position: "sticky", 
                 top: "73.6px",
                 zIndex:"2"
             }
        });

        //Success Rate
        props.CurrencyList.forEach(curr => {

            dateColResult.push({
                columnTitle: `${curr.code}`,
                dataColumn: `successRate_${data.accountType}_${data.accountId}_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            });
        });
        dateColResult.push({
            columnTitle: 'Total Success Rate',
             dataColumn: `successRateSummary_${data.accountType}_${data.accountId}`,
             body:{
                 attr:{
                     class:"text-right",
                     bgcolor:"LightGray"
                 },
                 style:{}
             },
             style:{
                 position: "sticky", 
                 top: "73.6px",
                 zIndex:"2"
             }
        });

        //Total FTDCount
        props.CurrencyList.forEach(curr => {

            dateColResult.push({
                columnTitle: `${curr.code}`,
                dataColumn: `ftdCount_${data.accountType}_${data.accountId}_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            });
        });
        dateColResult.push({
            columnTitle: 'Total FTD Count',
             dataColumn: `ftdCountSummary_${data.accountType}_${data.accountId}`,
             body:{
                 attr:{
                     class:"text-right",
                     bgcolor:"LightGray"
                 },
                 style:{}
             },
             style:{
                 position: "sticky", 
                 top: "73.6px",
                 zIndex:"2"
             }
        });
    });

    return dateColResult;
}

//#endregion


const Panel = (props) => {

    const groupColumnDefs = dataGroupColumnDefs(props);
    const columnDefs = dataColumnDefs(props);

    let groupHeaderRows = [];
    let headerRows = [];

    let getDateItem=props.DateDatas.find(x=>x.accountType==props.AccountDataShowLists.AccountType 
        && x.accountId==props.AccountDataShowLists.AccountId);

    let DateData=[];

    if(getDateItem)
    {
        DateData.push(getDateItem);
    }

    const accountColumnDefs = dateDataAccountColumnDefs(DateData,props);
    const dateGroupColumnDefs = dateDataGroupColumnDefs(DateData,props);
    const dateColumnDefs = dateDataColumnDefs(DateData,props);

    let accountHeaderRows = [];
    let dateGroupHeaderRows = [];
    let dateHeaderRows = [];
    let dateBodyRows = [];
    let dateTotalRows = [];

    //#region  Summary Data

    groupColumnDefs.forEach((element, idx) => {
        groupHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`}
                scope="col"
                className={element && element.attr && element.attr.class ? element.attr.class : ''} 
                style={element.style ? element.style : ''}
                colSpan={element.colSpan}>
                {element.columnTitle}
            </th>
        );
    });

    columnDefs.forEach((element, idx) => {
        headerRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col" 
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}
                style={element.style ? element.style : ''}>
                {element.columnTitle}
            </th>
        );
    });

    let bodyRows = props.Datas.map((data, idx) => {
        let row = columnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            if(element.columnTitle == "Detail")
            {
                return (
                    <td key={`td-${element.dataColumn}-${idx}`} 
                        className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                        style={element.body && element.body.style ? element.body.style : ''}
                        bgcolor={element.body && element.body.attr && element.body.attr.bgcolor ? element.body.attr.bgcolor : ''}
                        >
                        {<button type="button" className="btn btn-info" style={{width:"40px",padding:"1px"}} onClick={() => props.SetAccountShowBtn(data.accountType,data.accountId,data.accountName)}>show</button>}
                    </td>
                );
            }
            else
            {
                return (
                    <td key={`td-${element.dataColumn}-${idx}`} 
                        className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                        style={element.body && element.body.style ? element.body.style : ''}
                        bgcolor={element.body && element.body.attr && element.body.attr.bgcolor ? element.body.attr.bgcolor : ''}
                        >
                        {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                    </td>
                );
            }
        });

        return (
            <tr key={`result-body-${idx}`}>
                {row}
            </tr>
        )
    });

    let totalRows = props.TotalResult.map((data, idx) => {
        let row = columnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <th key={`td-${element.dataColumn}-${idx}`}
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    style={element.body && element.body.style ? element.body.style : ''}
                    bgcolor="LemonChiffon">
                    {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                </th>
            );
        });

        return (
            <tr key={`result-body-${idx}`}>
                {row}
            </tr>
        )
    });

    //#endregion

    //#region Detail Data
console.log('AccountDataShowLists',props.AccountDataShowLists);
    if(getDateItem)
    {

        accountColumnDefs.forEach((element, idx) => {
            accountHeaderRows.push(
                <th key={`th-${element.dataColumn}-${idx}`}
                    scope="col"
                    className={element && element.attr && element.attr.class ? element.attr.class : ''} 
                    style={element.style ? element.style : ''}
                    colSpan={element.colSpan}>
                    {element.columnTitle}
                </th>
            );
        });
    
        dateGroupColumnDefs.forEach((element, idx) => {
            dateGroupHeaderRows.push(
                <th key={`th-${element.dataColumn}-${idx}`}
                    scope="col"
                    className={element && element.attr && element.attr.class ? element.attr.class : ''} 
                    style={element.style ? element.style : ''}
                    colSpan={element.colSpan}>
                    {element.columnTitle}
                </th>
            );
        });
    
        dateColumnDefs.forEach((element, idx) => {
            dateHeaderRows.push(
                <th key={`th-${element.dataColumn}-${idx}`} 
                    scope="col" 
                    className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}
                    style={element.style ? element.style : ''}>
                    {element.columnTitle}
                </th>
            );
        });
    
    
        dateBodyRows = props.DateList.map((data, idx) => {
            let row = dateColumnDefs.map((element, idx) => {
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
                <tr key={`dateResult-body-${idx}`}>
                    {row}
                </tr>
            )
        });
    
        dateTotalRows = props.DateTotalReuslt.map((data, idx) => {
            let row = dateColumnDefs.map((element, idx) => {
                let subData = {};
                if (element.body && element.body.subData) {
                    element.body.subData.forEach(s => { subData[s] = data[s] });
                }
                return (
                    <th key={`td-${element.dataColumn}-${idx}`}
                        className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                        style={element.body && element.body.style ? element.body.style : ''}
                        bgcolor="LemonChiffon">
                        {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                    </th>
                );
            });
    
            return (
                <tr key={`result-body-${idx}`}>
                    {row}
                </tr>
            )
        });

    }

    //#endregion 

    return (
        
        <div className="table">

            <div className="table-options">
         
                {/* <button type="button" className="btn btn-light" 
                        style={props.SummaryBtnDisabled ? {background:"black",color:"white"} : null}
                        onClick={() => props.SetShowDataBtn(props.SummaryBtnDisabled,props.DateBtnDisabled)} 
                        disabled={props.SummaryBtnDisabled}>Summary</button>
                <button type="button" className="btn btn-light" 
                        style={props.DateBtnDisabled ? {background:"black",color:"white"} : null}
                        onClick={() => props.SetShowDataBtn(props.SummaryBtnDisabled,props.DateBtnDisabled)} 
                        disabled={props.DateBtnDisabled}>ByDate</button> */}
                <button type="button" className="btn btn-approved"
                        // style={{position:"absolute",right:"23px"}}
                        onClick={props.ExportBtnDisabled ? null : props.ExportToExcel} 
                        disabled={props.ExportBtnDisabled}>Export Excel</button>
            </div>

            <table className="table table-striped" style={{width:"auto"}} hidden={!props.SummaryBtnDisabled}>
                <thead>
                    <tr>
                        <th colSpan="2" style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3",background:"#FFFF45",color:"#F00000"}} >Summary</th>
                        <th colSpan={(props.CurrencyList.length+1)*2} className="text-center" style={{position:"sticky", top:"0px" }}>Online Banking</th>
                        <th colSpan={(props.CurrencyList.length+1)*2} className="text-center" style={{position:"sticky", top:"0px" }}>Mobile Pay</th>
                        <th colSpan={(props.CurrencyList.length+1)*2} className="text-center" style={{position:"sticky", top:"0px" }}>Local Banking</th>
                        <th colSpan={(props.CurrencyList.length+1)*6} style={{position:"sticky", top:"0px" }}></th>
                    </tr>
                    <tr>
                        <th colSpan="2" style={{ position: "sticky", top: "36.8px",left:"0",height:"36.8px",zIndex:"3"}}></th>
                        {groupHeaderRows}
                    </tr>
                    <tr>
                        {headerRows}
                    </tr>
                </thead>
                <tbody>
                    {bodyRows}
                    {totalRows}
                </tbody>
            </table>
            
            <table className="table table-striped" style={{width:"auto"}} hidden={!props.DateBtnDisabled}>
                <thead>
                    <tr>
                        <th style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3"}}>Account</th>
                        {accountHeaderRows}
                    </tr>
                    <tr>
                        <th style={{ position: "sticky", top: "36.8px",left:"0",height:"36.8px",zIndex:"3", background:"#FFFF45",color:"#F00000"}}>ByDate</th>
                        {dateGroupHeaderRows}
                    </tr>
                    <tr>
                        {dateHeaderRows}
                    </tr>
                </thead>
                <tbody>
                    {dateBodyRows}
                    {dateTotalRows}
                </tbody>
            </table>

            <Modal
                show={props.AccoutDataShowStatus} handleClose={e => props.CloseBtn(props.AccoutDataShowStatus)}
                accountName={props.AccountDataShowLists.AccountName}
                accountHeaderRows={accountHeaderRows}
                dateGroupHeaderRows={dateGroupHeaderRows}
                dateHeaderRows={dateHeaderRows}
                dateBodyRows={dateBodyRows}
                dateTotalRows={dateTotalRows}/>
            
        </div>
    )
}

export default Panel;